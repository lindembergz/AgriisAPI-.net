# Script para atualizar todos os testes de integração para usar SimpleTestServer

$testFiles = Get-ChildItem -Path "tests/Agriis.Tests.Integration" -Filter "Test*.cs" | Where-Object { $_.Name -ne "ExampleIntegrationTest.cs" }

foreach ($file in $testFiles) {
    Write-Host "Atualizando $($file.Name)..."
    
    $content = Get-Content $file.FullName -Raw
    
    # Remove IClassFixture<TestWebApplicationFactory>
    $content = $content -replace ', IClassFixture<TestWebApplicationFactory>', ''
    
    # Remove o campo _jsonMatchers (já está no BaseTestCase)
    $content = $content -replace '\s*private readonly JsonMatchers _jsonMatchers;\s*', "`n"
    
    # Atualiza o construtor para não receber parâmetros
    $content = $content -replace 'public ([^(]+)\(TestWebApplicationFactory factory\) : base\(factory\)\s*\{[^}]*\}', 'public $1()
    {
    }'
    
    # Remove inicialização do _jsonMatchers no construtor
    $content = $content -replace '\s*_jsonMatchers = new JsonMatchers\(\);\s*', ''
    
    # Substitui _jsonMatchers por JsonMatchers (que vem do BaseTestCase)
    $content = $content -replace '_jsonMatchers', 'JsonMatchers'
    
    Set-Content -Path $file.FullName -Value $content -NoNewline
}

Write-Host "Atualização concluída!"