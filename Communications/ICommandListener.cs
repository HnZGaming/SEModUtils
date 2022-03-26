namespace HNZ.Utils.Communications
{
    public interface ICommandListener
    {
        bool ProcessCommandOnClient(Command command);
        void ProcessCommandOnServer(Command command);
    }
}