a=imread('wjk.jpg');  
subplot(221);  
imshow(a);  
title ('原始图像');  
Inoise=imnoise(a,'gaussian',0.1,0.004);%对图像加入高斯噪声  
subplot(222);  
imshow(Inoise);  
title('加入高斯噪声后的图像');  
imwrite(Inoise,'add_guaiss_noise.jpg','jpg')