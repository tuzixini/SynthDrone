a=imread('wjk.jpg');  
subplot(221);  
imshow(a);  
title ('ԭʼͼ��');  
Inoise=imnoise(a,'gaussian',0.1,0.004);%��ͼ������˹����  
subplot(222);  
imshow(Inoise);  
title('�����˹�������ͼ��');  
imwrite(Inoise,'add_guaiss_noise.jpg','jpg')