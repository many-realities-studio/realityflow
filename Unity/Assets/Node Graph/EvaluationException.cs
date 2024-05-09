namespace RealityFlow.NodeGraph
{
    [System.Serializable]
    public class EvaluationException : System.Exception
    {
        public EvaluationException() { }
        public EvaluationException(string message) : base(message) { }
        public EvaluationException(string message, System.Exception inner) : base(message, inner) { }
        protected EvaluationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class TooManyInputsException : EvaluationException
    {
        public TooManyInputsException() : base("Too many inputs to data input port") { }
        public TooManyInputsException(string message) : base(message) { }
        public TooManyInputsException(string message, System.Exception inner) : base(message, inner) { }
        protected TooManyInputsException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class InvalidDataFlowException : EvaluationException
    {
        public InvalidDataFlowException() : base("Non-pure dependency of node is not executed first") { }
        public InvalidDataFlowException(string message) : base(message) { }
        public InvalidDataFlowException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidDataFlowException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}