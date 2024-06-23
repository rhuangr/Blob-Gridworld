
public class StateAction
{
    public Tile preyPosition;
    public Tile predatorPosition;
    public Tile applePosition;
    public int action;
  
    public StateAction(Tile preyPos, Tile predatorPos, Tile applePos, int act)
    {
        preyPosition = preyPos;
        predatorPosition = predatorPos;
        applePosition = applePos;
        action = act;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        StateAction other = (StateAction)obj;

        if (!preyPosition.Equals(other.preyPosition) || !predatorPosition.Equals(other.predatorPosition)){
            return false;
        }
        if (!applePosition.Equals(other.applePosition)){
            return false;
        }
        if (!action.Equals(other.action)){
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hash = preyPosition.GetHashCode() ^ predatorPosition.GetHashCode();
        hash = hash ^ action.GetHashCode();
        hash = hash ^ applePosition.GetHashCode();
        
        return hash;
    }
}