using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public class PlayerManager : SingletonMonobehavior<PlayerManager>
{
    public Player LocalPlayer => LobbyManager.Instance.CurrentLobby
                                                        .Players
                                                        .FirstOrDefault(
                                                            p => p.Id.Equals(AuthenticationService.Instance.PlayerId)
                                                        );
}