"building the frontend"
java -jar antlr-4.13.1-complete.jar -package ScratchScript.Compiler.Frontend.GeneratedVisitor -Dlanguage=CSharp ../Grammar/ScratchScriptParser.g4 ../Grammar/ScratchScriptLexer.g4 -visitor -o ../../Frontend/GeneratedVisitor
"building the backend"
java -jar antlr-4.13.1-complete.jar -package ScratchScript.Compiler.Backend.GeneratedVisitor -Dlanguage=CSharp ../Grammar/ScratchIR.g4 -visitor -o ../../Backend/GeneratedVisitor
"building the constant evaluator"
java -jar antlr-4.13.1-complete.jar -package ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.GeneratedVisitor -Dlanguage=CSharp ../Grammar/ScratchCE.g4 -visitor -o ../../Frontend/Optimization/ConstantEvaluator/GeneratedVisitor