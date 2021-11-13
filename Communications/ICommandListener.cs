namespace HNZ.Utils.Communications
{
    public interface ICommandListener
    {
        bool TryProcessClientCommand(Command command);
        void ProcessServerCommand(Command command);
    }
}