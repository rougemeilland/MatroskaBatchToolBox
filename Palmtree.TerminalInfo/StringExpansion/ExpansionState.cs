using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Palmtree.IO;

namespace Palmtree.Terminal.StringExpansion
{
    internal class ExpansionState
    {
        private class ArgumentsAccesser
            : IArgumentIndexer<char, ExpansionParameter>
        {
            private readonly char _startIndex;
            private readonly char _endIndex;
            private readonly ExpansionParameter[] _parameters;

            public ArgumentsAccesser(object[] parameters, char startIndex, char endIndex)
            {
                if (startIndex > endIndex)
                    throw new ArgumentException($"{nameof(startIndex)} > {nameof(endIndex)}");

                if (parameters.Length > endIndex - startIndex + 1)
                    throw new ExpansionBadArgumentExceptionException($"Too many parameters.: {parameters.Length}");

                _startIndex = startIndex;
                _endIndex = endIndex;
                _parameters =
                    parameters
                    .Select(arg =>
                        arg switch
                        {
                            int intArg => new ExpansionNumberParameter(intArg) as ExpansionParameter,
                            char charArg => new ExpansionNumberParameter(charArg),
                            bool boolArg => new ExpansionNumberParameter(boolArg),
                            string stringArgument => new ExpansionStringParameter(stringArgument),
                            _ => throw new ExpansionBadArgumentExceptionException($"Not supported argument type.: arg=\"{arg}\", type={arg.GetType().FullName}"),
                        })
                    .ToArray();
            }

            public ExpansionParameter this[char index]
            {
                get =>
                    index.IsInClosedInterval(_startIndex, _endIndex) && index - _startIndex < _parameters.Length
                    ? _parameters[index - _startIndex]
                    : throw new ExpansionBadArgumentExceptionException($"Failed to get argument '{index}'.");

                set
                {
                    if (index.IsOutOfClosedInterval(_startIndex, _endIndex) || index - _startIndex >= _parameters.Length)
                        throw new ExpansionBadArgumentExceptionException($"Failed to set argument '{index}'.");

                    _parameters[index - _startIndex] = value;
                }
            }

            public bool TryGet(char index, [NotNullWhen(true)] out ExpansionParameter? value)
            {
                if (index.IsInClosedInterval(_startIndex, _endIndex) && index - _startIndex < _parameters.Length)
                {
                    value = _parameters[index - _startIndex];
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
        }

        private class VariableAccesser
            : IIndexer<char, ExpansionParameter>
        {
            private readonly char _startIndex;
            private readonly char _endIndex;
            private readonly ExpansionParameter[] _values;

            public VariableAccesser(char startIndex, char endIndex)
            {
                if (startIndex > endIndex)
                    throw new ArgumentException($"{nameof(startIndex)} > {nameof(startIndex)}");

                _startIndex = startIndex;
                _endIndex = endIndex;
                _values = new ExpansionParameter[_endIndex - startIndex + 1];
            }

            public ExpansionParameter this[char index]
            {
                get
                {
                    if (index.IsOutOfClosedInterval(_startIndex, _endIndex))
                        throw new ExpansionStringSyntaxErrorExceptionException($"'{index}' is an invalid variable name.");

                    var value = _values[index - _startIndex];
                    return
                        value is not null
                        ? value
                        : throw new ExpansionStringSyntaxErrorExceptionException($"Failed to get variable '{index}'.");
                }

                set
                {
                    if (index.IsOutOfClosedInterval(_startIndex, _endIndex))
                        throw new ExpansionStringSyntaxErrorExceptionException($"'{index}' is an invalid variable name.");
                    if (value is null)
                        throw new InvalidOperationException($"Must not set null in '{index}'.");

                    _values[index - _startIndex] = value;
                }
            }
        }

        private readonly IPrefetchableTextReader _reader;
        private readonly Stack<ExpansionParameter> _stack;

        public ExpansionState(string sourceString, object[] args)
        {
            SourceString = sourceString;
            _reader = new ExpansionReader(sourceString);
            Arguments = new ArgumentsAccesser(args, '1', '9');
            _stack = new Stack<ExpansionParameter>();
            DynamicValues = new VariableAccesser('a', 'z');
            StaticValues = new VariableAccesser('A', 'Z');
        }

        public string SourceString { get; }
        public IArgumentIndexer<char, ExpansionParameter> Arguments { get; }
        public IIndexer<char, ExpansionParameter> DynamicValues { get; }
        public IIndexer<char, ExpansionParameter> StaticValues { get; }
        public char? ReadCharOrNull() => _reader.Read();

        public char ReadChar()
        {
            var c = _reader.Read();
            return
                c is not null
                ? c.Value
                : throw new InvalidOperationException("Unexpected end of string.");
        }

        public bool ReaderStartsWith(string s) => _reader.StartsWith(s);
        public void Push(ExpansionParameter value) => _stack.Push(value);
        public ExpansionParameter Pop() => _stack.Pop();
    }
}
