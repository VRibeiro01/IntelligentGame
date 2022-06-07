using JAZG.Model.Players;

namespace JAZG.Model.Players
{
    public abstract class CustomHuman : Human
    {
        public abstract override void Tick();
    }
}

public class PleaseRenameYourHuman : CustomHuman
{
    public override void Tick()
    {
        throw new System.NotImplementedException();
    }
}