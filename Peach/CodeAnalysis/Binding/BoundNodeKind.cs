﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peach.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        ParenthesisedExpression,
        VariableExpression,
        AssignmentExpression
    }
}