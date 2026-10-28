using System;
using System.Collections.Generic;
using Game.Character;
using Game.Configurations;
using UnityEngine;
using VContainer.Unity;

namespace Game.Runtime
{
    public class BattleController : IStartable
    {
        private const float Y = -1;
        private const float StartX = 8;
        private const float CharacterDistance = 2;

        private readonly ICharacterFactory _characterFactory;
        private readonly TeamsConfiguration _teamsConfiguration;

        // Index 0 = frontline (closest to the enemy). Bodies do not move; only who counts as front changes.
        private readonly List<CharacterActor> _left = new();
        private readonly List<CharacterActor> _right = new();

        
        public BattleController(ICharacterFactory characterFactory, TeamsConfiguration teamsConfiguration)
        {
            _characterFactory = characterFactory;
            _teamsConfiguration = teamsConfiguration;
        }
        
        public void Start()
        {
            _createTeam(ETeamId.Left, _teamsConfiguration.TeamLeft, _left);
            _createTeam(ETeamId.Right, _teamsConfiguration.TeamRight, _right);      
        }
        
        private void _createTeam(
            ETeamId team, 
            IReadOnlyList<CharacterConfiguration> configurations, 
            ICollection<CharacterActor> actors)
        {
            if (configurations == null || configurations.Count == 0)
            {
                throw new Exception($"configurations from team {team} is empty");
            }

            // Slot 0 at front line (near center), higher slots go outward. No Reverse — list[i].Slot == i.
            float outward = team is ETeamId.Left ? -1f : 1f;
            float frontX = outward * (StartX - (configurations.Count - 1) * CharacterDistance);

            for (int slot = 0; slot < configurations.Count; slot++)
            {
                CharacterConfiguration characterConfiguration = configurations[slot];

                if (characterConfiguration == null)
                {
                    throw new Exception($"character configuration #{slot} from team {team} is null");
                }

                float x = frontX + outward * slot * CharacterDistance;
                CharacterActor actor = _characterFactory.Create(characterConfiguration, team, slot);
                actors.Add(actor);
                actor.WorldView.SetPosition(new Vector3(x, Y, 0), team);
            }
        } 
    }
}