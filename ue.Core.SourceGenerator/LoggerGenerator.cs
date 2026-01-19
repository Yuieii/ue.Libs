using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ue.Core.SourceGenerator
{
    [Generator]
    public class LoggerGenerator : IIncrementalGenerator
    {
        private readonly DiagnosticDescriptor _needPartial = new DiagnosticDescriptor(
#pragma warning disable RS2008
            "UE0001", 
#pragma warning restore RS2008
            "This type needs to be partial in order to add logger for it",
            "This type needs to be partial in order to add logger for it", 
            "Logging", DiagnosticSeverity.Error, true);
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (ctx, compilation) =>
            {
                var verbose = new StringBuilder();
                verbose.AppendLine($"// Copyright (c) {DateTimeOffset.Now.Year} Yuieii.");
                verbose.AppendLine($"// Generated at {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}");
                verbose.AppendLine();

                foreach (var tree in compilation.SyntaxTrees)
                {
                    var model = compilation.GetSemanticModel(tree);
                    foreach (var typedef in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                    {
                        var symbol = ModelExtensions.GetDeclaredSymbol(model, typedef)!;
                        var attributes = symbol.GetAttributes();
                        if (attributes.Any(a =>
                                a.AttributeClass!.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ==
                                "ue.Core.Attributes.RegisterLoggerAttribute"))
                        {
                            if (!typedef.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                            {
                                ctx.ReportDiagnostic(Diagnostic.Create(_needPartial, typedef.Identifier.GetLocation(), typedef.Identifier.ValueText));
                                continue;
                            }
                            
                            var ns = symbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat
                                .CSharpErrorMessageFormat);

                            var tParam = typedef.TypeParameterList;
                            if (tParam != null)
                            {
                                tParam = SyntaxFactory.TypeParameterList(
                                    SyntaxFactory.SeparatedList(
                                        tParam.Parameters.Select(p =>
                                            p.WithoutTrivia().WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>())
                                        )));
                            }
                            
                            var syntax = typedef
                                .WithModifiers(SyntaxFactory.TokenList(typedef.Modifiers.Select(m => m.WithoutTrivia())))
                                .WithoutLeadingTrivia()
                                .WithoutTrailingTrivia()
                                .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
                                .WithSemicolonToken(SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken))
                                .WithBaseList(null)
                                .WithParameterList(null)
                                .WithTypeParameterList(tParam)
                                .WithConstraintClauses(SyntaxFactory.List<TypeParameterConstraintClauseSyntax>())
                                .WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>())
                                .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                                        SyntaxFactory.PropertyDeclaration(
                                                SyntaxFactory.ParseTypeName("global::Microsoft.Extensions.Logging.ILogger"),
                                                "Logger")
                                            .WithModifiers(SyntaxFactory.TokenList(
                                                SyntaxFactory.Token(SyntaxKind.PrivateKeyword)
                                                    .WithTrailingTrivia(SyntaxFactory.Space),
                                                SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                                                    .WithTrailingTrivia(SyntaxFactory.Space)
                                                ))
                                            .WithAccessorList(SyntaxFactory.AccessorList(
                                                SyntaxFactory.SingletonList(
                                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                                    )
                                                )
                                            )
                                            .WithInitializer(SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.InvocationExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ParseTypeName(
                                                                    "global::ue.Core.LogUtils"),
                                                                SyntaxFactory.IdentifierName(
                                                                    "GetLogger")))
                                                        .WithArgumentList(
                                                            SyntaxFactory.ArgumentList(
                                                                SyntaxFactory.SingletonSeparatedList(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.LiteralExpression(
                                                                            SyntaxKind
                                                                                .StringLiteralExpression,
                                                                            SyntaxFactory.Literal(
                                                                                symbol.ToDisplayString(
                                                                                    SymbolDisplayFormat
                                                                                        .CSharpErrorMessageFormat)
                                                                            )
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                )
                                            )
                                            .WithSemicolonToken(
                                                SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                            .WithLeadingTrivia(
                                                SyntaxFactory.ParseLeadingTrivia("/// <summary>The default logger for this type.</summary>\n"))
                                            .NormalizeWhitespace()
                                    )
                                )
                                .NormalizeWhitespace();

                            verbose.AppendLine(SyntaxFactory
                                .NamespaceDeclaration(SyntaxFactory.IdentifierName(ns))
                                .WithUsings(SyntaxFactory.List(tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>()
                                    .Select(c => c.WithoutLeadingTrivia().WithoutTrailingTrivia())))
                                .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(syntax))
                                .NormalizeWhitespace().ToFullString());
                        }
                    }
                }
                
                ctx.AddSource("Loggers.g.cs", verbose.ToString());
            });
        }
    }
}