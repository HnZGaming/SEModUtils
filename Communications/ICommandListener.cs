namespace HNZ.Utils.Communications
{
    public interface ICommandListener
    {
        void ProcessCommandOnServer(Command command);
    }
}