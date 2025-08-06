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
            // Добавляем пользователя к группе презентации
            await Groups.AddToGroupAsync(Context.ConnectionId, $"presentation_{presentationId}");

            // Добавляем пользователя в хранилище
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

            // Уведомляем всех участников о новом пользователе
            var currentUsers = presentationUsers.Values.ToList();
            await Clients.Group($"presentation_{presentationId}")
                .SendAsync("UpdateUsersList", currentUsers);

            // Уведомляем о подключении
            await Clients.Group($"presentation_{presentationId}")
                .SendAsync("UserJoined", user);
        }
        public async Task UserRoleChanged(string presentationId, string userName, string newRole)
        {
            // Уведомляем всех участников об изменении роли пользователя
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
            // Удаляем пользователя из группы
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"presentation_{presentationId}");

            // Удаляем пользователя из хранилища
            if (_presentationUsers.TryGetValue(presentationId, out var presentationUsers))
            {
                if (presentationUsers.TryRemove(Context.ConnectionId, out var removedUser))
                {
                    // Уведомляем оставшихся участников
                    var currentUsers = presentationUsers.Values.ToList();
                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UpdateUsersList", currentUsers);

                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UserLeft", removedUser);
                }

                // Если в презентации никого не осталось, удаляем её из хранилища
                if (presentationUsers.IsEmpty)
                {
                    _presentationUsers.TryRemove(presentationId, out _);
                }
            }
        }

        public async Task AddSlide(string presentationId)
        {
            // Уведомляем всех участников о добавлении нового слайда
            await Clients.Group($"presentation_{presentationId}")
                .SendAsync("SlideAdded", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Ищем пользователя во всех презентациях и удаляем его
            foreach (var kvp in _presentationUsers)
            {
                var presentationId = kvp.Key;
                var presentationUsers = kvp.Value;

                if (presentationUsers.TryRemove(Context.ConnectionId, out var removedUser))
                {
                    // Уведомляем оставшихся участников
                    var currentUsers = presentationUsers.Values.ToList();
                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UpdateUsersList", currentUsers);

                    await Clients.Group($"presentation_{presentationId}")
                        .SendAsync("UserLeft", removedUser);

                    // Если в презентации никого не осталось, удаляем её из хранилища
                    if (presentationUsers.IsEmpty)
                    {
                        _presentationUsers.TryRemove(presentationId, out _);
                    }
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Получить список текущих пользователей в презентации
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