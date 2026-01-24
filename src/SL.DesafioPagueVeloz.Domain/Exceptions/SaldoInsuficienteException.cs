namespace SL.DesafioPagueVeloz.Domain.Exceptions
{
    public class SaldoInsuficienteException : Exception
    {
        public SaldoInsuficienteException(string message) : base(message) { }
    }
}
