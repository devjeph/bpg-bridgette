using Bridgette.Api.Dtos;
using Bridgette.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Google.Apis.HangoutsChat.v1.Data;

namespace Bridgette.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITimesheetCheckService _timesheetCheckService;

    public ChatController(IUserService userService, ITimesheetCheckService timesheetCheckService)
    {
        _userService = userService;
        _timesheetCheckService = timesheetCheckService;
    }

    [HttpPost("events")]
    public async Task<IActionResult> HandleEvent([FromBody] GoogleChatEvent chatEvent)
    {
        object response;
        switch (chatEvent.Type)
        {
            case "ADDED_TO_SPACE":
                await _userService.OnboardUserAsync(new Data.Models.User
                {
                    GoogleChatUserId = chatEvent.User.Name,
                    UserDisplayName = chatEvent.User.DisplayName,
                    UserEmail = chatEvent.User.Email,
                });
                response = CreateConsentCard();
                break;
            
            case "MESSAGE":
                response = await HandleMessageEvent(chatEvent);
                break;
            
            case "CARD_CLICKED":
                if (chatEvent.Common?.InvokedFunction == "grant_consent")
                {
                    await _userService.GrantConsentAsync(chatEvent.User.Name);
                    response = new
                    {
                        text = "You can now message me."
                    };
                }
                else
                {
                    response = new
                    {
                        text = "Action not recognized."
                    };

                }
                break;
            
            default:
                return Ok();
        }
        return Ok(response);
    }

    private async Task<object> HandleMessageEvent(GoogleChatEvent chatEvent)
    {
        var commandText = chatEvent.Message.Text.Trim();
        string responseText;

        // ReSharper disable once StringLiteralTypo
        if (commandText.StartsWith("/registertimesheet"))
        {
            // ReSharper disable once StringLiteralTypo
            var spreadsheetId = commandText.Replace("/registertimesheet", "").Trim();
            if (string.IsNullOrWhiteSpace(spreadsheetId))
            {
                // ReSharper disable once StringLiteralTypo
                responseText = "Please provide your timesheet spreadsheet ID. Usage: /registertimesheet <ID>";
            }
            else
            {
                await _userService.RegisterTimesheetAsync(chatEvent.User.Name, spreadsheetId);
                responseText = "Your timesheet has been registered successfully!";
            }
        }
        // ReSharper disable once StringLiteralTypo
        else if (commandText.StartsWith("/checktimesheet"))
        {
            responseText = await _timesheetCheckService.ExecuteOnDemandCheckAsync(chatEvent.User.Name);
        }
        // ReSharper disable once StringLiteralTypo
        else if (commandText.StartsWith("/checkalltimesheets"))
        {
            if (!await _userService.IsUserAdminAsync(chatEvent.User.Name))
            {
                responseText = "Access Denied: You are not authorized to perform this action.";
            }
            else
            {
                var month = Regex.Match(commandText, @"\d{6}").Value; // Extracts YYYYMM
                if (string.IsNullOrEmpty(month))
                {
                    responseText = "Please specify a month. Usage: /checkalltimesheet YYYYMM";
                }
                else
                {
                    responseText = await _timesheetCheckService.ExecuteOnDemandCheckAsync(month);
                }

            }
        }
        else
        {
            responseText = "Command not recognized.";
        }

        return new
        {
            text = responseText
        };
    }

    private object CreateConsentCard()
    {
        return new
        {
            cardsV2 = new[]
            {
                new
                {
                    cardId = "consentCard",
                    card = new
                    {
                        header = new
                        {
                            title = "Hello! I'm Bridgette.",
                            subtitle = "your BPG Assistant Bot",
                        },
                        sections = new[]
                        {
                          new
                          {
                              widgets = new object[]
                              {
                                  new
                                  {
                                      textParagraph = new
                                      {
                                          text = "For you to interact, you must agree to allow me to access your registered Timesheet Spreadsheet ID. Please click below to provide consent."
                                      }
                                  },
                                  new
                                  {
                                      buttonList = new
                                      {
                                          buttons = new[]
                                          {
                                              new
                                              {
                                                  text = "Agree & Continue",
                                                  onClick = new
                                                  {
                                                      action = new
                                                      {
                                                          function = "grant_consent"
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                          }  
                        }
                    }
                }
            }
        };
    }
}