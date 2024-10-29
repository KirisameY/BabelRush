using System.Text;

namespace BabelRush.Generator;

public class IndentStringBuilder(string indentContent)
{
    public IndentStringBuilder(int indentLength = 4) : this(new string(' ', indentLength)) { }

    private readonly StringBuilder _builder = new();
    private int _currentLevel;
    private string _current = string.Empty;

    private bool _currentLineIntended = false;

    private void UpdateCurrent() =>
        _current = string.Concat(Enumerable.Repeat(indentContent, _currentLevel));
    
    public void IncreaseIndent()
    {
        _currentLevel++;
        UpdateCurrent();
    }

    public void DecreaseIndent()
    {
        _currentLevel--;
        UpdateCurrent();
    }

    public readonly struct IndentDisposable(IndentStringBuilder builder) : IDisposable
    {
        public void Dispose()
        {
            builder.DecreaseIndent();
        }
    }

    public IndentDisposable Indent()
    {
        IncreaseIndent();
        return new(this);
    }

    public IndentStringBuilder Append(string content)
    {
        if (!_currentLineIntended)
        {
            _builder.Append(_current);
            _currentLineIntended = true;
        }
        _builder.Append(content);
        return this;
    }

    public IndentStringBuilder AppendLine(string content)
    {
        if (!_currentLineIntended)
            _builder.Append(_current);
        _builder.AppendLine(content);
        _currentLineIntended = false;
        return this;
    }

    public override string ToString() => _builder.ToString();
}