    using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Task6.Models;

    namespace Task6.Hubs
    {
        public class PresentationHub : Hub
        {
            public async Task JoinGroup(string groupName)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }

            public async Task LeaveGroup(string groupName)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
            public async Task SendPresentationToGroup(string groupName, string title, string author, string date)
            {
                await Clients.Group(groupName).SendAsync("ReceivePresentation", title, author, date);
            }
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConnectedUser>> _presentationUsers
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConnectedUser>>();

        public async Task JoinPresentation(string presentationId, string userName, string userRole)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, $"presentation_{presentationId}");


            var presentationUsers = _presentationUsers.GetOrAdd(presentationId,
                _ => new ConcurrentDictionary<string, ConnectedUser>());

            var user = new ConnectedUser
            {
                ConnectionId = Context.ConnectionId,
                UserName = userName,
                Role = userRole,
                JoinedAt = DateTime.UtcNow
            };

            presentationUsers.AddOrUpdate(Context.ConnectionId, user, (key, oldValue) => user);

            var currentUsers = presentationUsers.Values.ToList();
            await Clients.Group($"presentation_{presentationId}")
                .SendAsync("UpdateUsersList", currentUsers);

            await Clients.Group($"presentation_{presentationId}")
                .SendAsync("UserJoined", user);
        }
        public async Task UserRoleChanged(string presentationId, string userName, string newRole)
        {

            await Clients.Group($"presentation_{presentationId}")
                .SendAsync("UserRoleChanged", userName, newRole);
        }
        public async Task SlideAdded(string presentationId,Slide slide)
        {
            await Clients.Group($"presentation_{presentationId}")
                    .SendAsync("SlideAdded", slide, Context.ConnectionId);
        }
        public async Task SlideDeleted(string presentationId, string slideId)
        {
            await Clients.Group($"presentation_{presentationId}")
                    .SendAsync("SlideDeleted", slideId, Context.ConnectionId.ToString());
        }
        public async Task LeavePresentation(string presentationId)
        {

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"presentation_{presentationId}");


            if (_presentationUsers.TryGetValue(presentationId, out var presentationUsers))
            {
                if (presentationUsers.TryRemove(Context.ConnectionId, out var removedUser))
                {

                    var currentUsers = presentationUsers.Values.ToList();
                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UpdateUsersList", currentUsers);

                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UserLeft", removedUser);
                }


                if (presentationUsers.IsEmpty)
                {
                    _presentationUsers.TryRemove(presentationId, out _);
                }
            }
        }

        public async Task AddSlide(string presentationId)
        {

            await Clients.Group($"presentation_{presentationId}")
                .SendAsync("SlideAdded", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            foreach (var kvp in _presentationUsers)
            {
                var presentationId = kvp.Key;
                var presentationUsers = kvp.Value;

                if (presentationUsers.TryRemove(Context.ConnectionId, out var removedUser))
                {

                    var currentUsers = presentationUsers.Values.ToList();
                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UpdateUsersList", currentUsers);

                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UserLeft", removedUser);


                    if (presentationUsers.IsEmpty)
                    {
                        _presentationUsers.TryRemove(presentationId, out _);
                    }
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task GetCurrentUsers(string presentationId)
        {
            if (_presentationUsers.TryGetValue(presentationId, out var presentationUsers))
            {
                var currentUsers = presentationUsers.Values.ToList();
                await Clients.Caller.SendAsync("UpdateUsersList", currentUsers);
            }
            else
            {
                await Clients.Caller.SendAsync("UpdateUsersList", new List<ConnectedUser>());
            }
        }
        public async Task TextElementAdded(string presentationId,TextElement textElement)
        {
            await Clients.Group($"presentation_{presentationId}").SendAsync("TextElementAdded",textElement, Context.ConnectionId);
        }

        public async Task TextElementUpdated(string presentationId, TextElement textElement)
        {
            await Clients.Group($"presentation_{presentationId}").SendAsync("TextElementUpdated", textElement, Context.ConnectionId);
        }

        public async Task TextElementDeleted(string presentationId, TextElement textElement)
        {
            
            await Clients.Group($"presentation_{presentationId}").SendAsync("TextElementDeleted", textElement, Context.ConnectionId);
        }

        public async Task TextElementPositionUpdated(string presentationId, string slideId, string textElementId, string positionX, string positionY)
        {
            
                await Clients.Group($"presentation_{presentationId}").SendAsync("TextElementPositionUpdated", slideId, textElementId, positionX, positionY, Context.ConnectionId.ToString());
            
        }
    }

    }   