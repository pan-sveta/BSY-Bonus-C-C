namespace Pasture.Messages;

public interface IMessage<T>
{
    public static abstract bool TryParse(string message, out T assignmentMessage);
    public string GetTransportFormat();
}