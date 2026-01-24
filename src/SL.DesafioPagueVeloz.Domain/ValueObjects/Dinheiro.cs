namespace SL.DesafioPagueVeloz.Domain.ValueObjects
{
    public sealed record Dinheiro
    {
        public decimal Valor { get; }

        private Dinheiro(decimal valor)
        {
            Valor = valor;
        }

        public static Dinheiro Criar(decimal valor)
        {
            if (valor < 0)
                throw new ArgumentException("Valor não pode ser negativo", nameof(valor));

            return new Dinheiro(Math.Round(valor, 2));
        }

        public static Dinheiro Zero => new(0);

        public static Dinheiro operator + (Dinheiro a, Dinheiro b) => Criar(a.Valor + b.Valor);
        public static Dinheiro operator - (Dinheiro a, Dinheiro b) => Criar(a.Valor - b.Valor);
        public static bool operator > (Dinheiro a, Dinheiro b) => a.Valor > b.Valor;
        public static bool operator < (Dinheiro a, Dinheiro b) => a.Valor < b.Valor;
        public static bool operator >= (Dinheiro a, Dinheiro b) => a.Valor >= b.Valor;
        public static bool operator <= (Dinheiro a, Dinheiro b) => a.Valor <= b.Valor;

        public override string ToString() => Valor.ToString("C2");
    }
}
