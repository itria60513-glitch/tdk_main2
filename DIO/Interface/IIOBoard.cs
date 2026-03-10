namespace DIO
{
    /// <summary>
    /// Aggregate interface combining both digital input (<see cref="IDI"/>) and
    /// digital output (<see cref="IDO"/>) capabilities into a single DIO board contract.
    /// </summary>
    public interface IIOBoard : IDI, IDO
    {
    }
}
