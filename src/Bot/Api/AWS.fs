module Api.AWS

let getObject (client: Amazon.S3.AmazonS3Client) (bucket: string) (filepath: string) =
  let request = new Amazon.S3.Model.GetObjectRequest(BucketName=bucket, Key=filepath)
  client.GetObjectAsync(request)
