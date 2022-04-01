﻿using VQoiSharp;
using VQoiSharp.Codec;
using StbImageSharp;

using System.IO;
public static class Program
{
    public static void Main(string[] args)
    {
        if(true){
            string imagePath = "/home/bluesillybeard/VisualStudioCodeProjects/Voxelesque/src/Resources/stripes.png";

            var img = ImageResult.FromMemory(File.ReadAllBytes(imagePath), ColorComponents.RedGreenBlueAlpha);
            var qoiImage = new VQoiImage(img.Data, img.Width, img.Height, (VQoiChannels)img.Comp);
            byte[] qoiData = VQoiEncoder.Encode(qoiImage, true);
            
            // saving image
            File.WriteAllBytes(imagePath+".vqoi", qoiData);

            byte[] qoiFileData = File.ReadAllBytes(imagePath+".vqoi");
            var fileImage = VQoiDecoder.Decode(qoiFileData);
            byte[] fileImageData = fileImage.Data;
            byte[] qoiImageData = qoiImage.Data;
            for(int i=0; i<fileImage.Data.Length; i++){
                if(fileImageData[i] != qoiImageData[i]){
                    System.Console.WriteLine($"{fileImageData[i]}, {qoiImageData[i]}, {i}");
                }
            }
        }
        
    }
}