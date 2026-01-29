namespace SL.DesafioPagueVeloz.Api.Tests.Fixtures
{
    public static class DocumentoHelper
    {
        // CPFs válidos para testes
        private static readonly List<string> CpfsDisponiveis = new()
        {
            "52998224725",
            "29537990841",
            "95524361503",
            "32428909128",
            "47123586964",
            "71428793860",
            "31862581040",
            "85914237605",
            "12345678909",
            "98765432100",
            "11144477735",
            "88877766644",
            "22233344455",
            "55566677788",
            "99988877766",
            "44455566677",
            "77788899900",
            "33344455566",
            "66677788899",
            "00011122233",
            "11122233344",
            "22233344456",
            "33344455567",
            "44455566678",
            "55566677789",
            "66677788890",
            "77788899901",
            "88899900012",
            "99900011123",
            "00011122234"
        };

        private static int _cpfIndex = 0;
        private static readonly object _lock = new();
        private static readonly HashSet<string> _cpfsUsados = new();

        public static string GerarCPFValido()
        {
            lock (_lock)
            {
                // Se todos os CPFs foram usados, resetar
                if (_cpfsUsados.Count >= CpfsDisponiveis.Count)
                {
                    _cpfsUsados.Clear();
                    _cpfIndex = 0;
                }

                // Pegar próximo CPF disponível
                string cpf;
                do
                {
                    cpf = CpfsDisponiveis[_cpfIndex % CpfsDisponiveis.Count];
                    _cpfIndex++;
                } while (_cpfsUsados.Contains(cpf) && _cpfsUsados.Count < CpfsDisponiveis.Count);

                _cpfsUsados.Add(cpf);
                return cpf;
            }
        }

        public static string GerarCNPJValido()
        {
            return "11222333000181"; // CNPJ válido de teste
        }
    }
}