using Game.Configurations;

namespace Game.Character
{
    public class CharacterActor
    {
        public CharacterWorldView WorldView { get; }
        public CharacterState State { get; }
        public CharacterConfiguration Configuration { get; }
        
        
        public CharacterActor(CharacterConfiguration configuration, CharacterWorldView worldView, CharacterState state)
        {
            Configuration = configuration;
                
            WorldView = worldView;

            State = state;
        }
    }
}