namespace GuimoSoft.Bus.Kafka.Consumer
{
    public static class Constants
    {
        public const string KEY_SEVERITY = "severidade";
        public const string KEY_MESSAGE = "mensagem";
        public const string KEY_ERROR_MESSAGE = "mensagem-erro";
        public const string KEY_ERROR_TYPE = "tipo-erro";
        public const string KEY_STACK_TRACE = "stack-trace";

        public const string SEVERIDADE_TRACE_STRING = "rastreamento";
        public const string SEVERIDADE_DEBUG_STRING = "debug";
        public const string SEVERIDADE_INFORMATION_STRING = "informacao";
        public const string SEVERIDADE_WARNING_STRING = "atencao";
        public const string SEVERIDADE_ERROR_STRING = "erro";
        public const string SEVERIDADE_CRITICAL_STRING = "critico";
        public const string SEVERIDADE_DEFAULT_STRING = "desconhecido";
    }
}
