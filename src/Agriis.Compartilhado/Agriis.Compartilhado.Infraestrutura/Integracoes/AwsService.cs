using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Agriis.Compartilhado.Infraestrutura.Integracoes;

public interface IAwsService
{
    Task<string> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType);
    Task<string> UploadFileAsync(string key, Stream fileStream, string contentType);
    Task<Stream> DownloadFileAsync(string bucketName, string key);
    Task<Stream> DownloadFileAsync(string key);
    Task<bool> DeleteFileAsync(string bucketName, string key);
    Task<bool> DeleteFileAsync(string key);
    Task<bool> FileExistsAsync(string bucketName, string key);
    Task<bool> FileExistsAsync(string key);
    Task<string> GetPreSignedUrlAsync(string bucketName, string key, TimeSpan expiration);
    Task<string> GetPreSignedUrlAsync(string key, TimeSpan expiration);
    Task<IEnumerable<string>> ListFilesAsync(string bucketName, string prefix = "");
    Task<IEnumerable<string>> ListFilesAsync(string prefix = "");
}

public class AwsService : IAwsService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<AwsService> _logger;
    private readonly string _defaultBucketName;

    public AwsService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<AwsService> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultBucketName = configuration["AwsSettings:S3BucketName"] ?? throw new ArgumentNullException("AwsSettings:S3BucketName");
    }

    public async Task<string> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            var response = await _s3Client.PutObjectAsync(request);
            
            _logger.LogInformation("Arquivo {Key} enviado com sucesso para o bucket {BucketName}. ETag: {ETag}", 
                key, bucketName, response.ETag);

            return $"https://{bucketName}.s3.amazonaws.com/{key}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar arquivo {Key} para o bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(string key, Stream fileStream, string contentType)
    {
        return await UploadFileAsync(_defaultBucketName, key, fileStream, contentType);
    }

    public async Task<Stream> DownloadFileAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            
            _logger.LogInformation("Arquivo {Key} baixado com sucesso do bucket {BucketName}", key, bucketName);
            
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Arquivo {Key} não encontrado no bucket {BucketName}", key, bucketName);
            throw new FileNotFoundException($"Arquivo {key} não encontrado no bucket {bucketName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar arquivo {Key} do bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string key)
    {
        return await DownloadFileAsync(_defaultBucketName, key);
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            
            _logger.LogInformation("Arquivo {Key} excluído com sucesso do bucket {BucketName}", key, bucketName);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir arquivo {Key} do bucket {BucketName}", key, bucketName);
            return false;
        }
    }

    public async Task<bool> DeleteFileAsync(string key)
    {
        return await DeleteFileAsync(_defaultBucketName, key);
    }

    public async Task<bool> FileExistsAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do arquivo {Key} no bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string key)
    {
        return await FileExistsAsync(_defaultBucketName, key);
    }

    public async Task<string> GetPreSignedUrlAsync(string bucketName, string key, TimeSpan expiration)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            
            _logger.LogInformation("URL pré-assinada gerada para arquivo {Key} no bucket {BucketName}, expira em {Expiration}", 
                key, bucketName, expiration);
            
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar URL pré-assinada para arquivo {Key} no bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<string> GetPreSignedUrlAsync(string key, TimeSpan expiration)
    {
        return await GetPreSignedUrlAsync(_defaultBucketName, key, expiration);
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string bucketName, string prefix = "")
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix,
                MaxKeys = 1000
            };

            var files = new List<string>();
            ListObjectsV2Response response;

            do
            {
                response = await _s3Client.ListObjectsV2Async(request);
                files.AddRange(response.S3Objects.Select(obj => obj.Key));
                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated);

            _logger.LogInformation("Listados {Count} arquivos no bucket {BucketName} com prefixo '{Prefix}'", 
                files.Count, bucketName, prefix);

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar arquivos no bucket {BucketName} com prefixo '{Prefix}'", bucketName, prefix);
            throw;
        }
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string prefix = "")
    {
        return await ListFilesAsync(_defaultBucketName, prefix);
    }
}