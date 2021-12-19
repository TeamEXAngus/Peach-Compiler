namespace Peach.CodeAnalysis
{
    internal sealed class BoundLabel
    {
        private static int _labelCount = 0;

        internal static BoundLabel GenerateLabel(string name = null)
        {
            var labelName = $"{++_labelCount}__{name ?? "Label"}";
            Debug.Log($"Created label {labelName}");
            return new BoundLabel(labelName);
        }

        internal BoundLabel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString() => Name;
    }
}