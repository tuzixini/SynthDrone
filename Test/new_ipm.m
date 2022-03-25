% 输入第一个是图像
% 第二个是消隐线在y轴（竖直方向）的坐标
% 输出第一个是得到的ipm图像
% 第二个是ipm图像中有效的部分，其实不用管，用不用都行
function [ipm,lim_y] = new_ipm(img,vpy)
%     figure;imshow(img);
if size(img,3) ==3
    im = double(img(:,:,1))/2+double(img(:,:,2))/2;
else
    im = img;
end
     imsize = size(im);
%     hold on;plot([imsize(2)/2,imsize(2)/2],[0,imsize(1)],'r');hold off;
%     [~,vpy] = ginput(1);
    alpha = 50;c = 50; h = 100;
    x_pad = 0.1;y_pad = 0.15;
    x_left = x_pad*imsize(2);x_right = (1-x_pad)*imsize(2);
    y_up = (imsize(1)-vpy)*y_pad+vpy;y_bottom = imsize(1);
    x = [x_left;x_left;x_right;x_right];
    y = [y_up;y_bottom;y_up;y_bottom];
%     hold on;plot(x,y,'r');hold off;
    sita = atan(tan(alpha*pi/180)*(1-2*vpy/imsize(1)))*180/pi;
    Param = [sita*pi/180,alpha*pi/180,c*pi/180,imsize(1),imsize(2),h];
    [ipminfo] = calc_ipminfo(x,y,Param);
    ipm = get_ipm(im,Param,ipminfo);
    lim_y = get_lim(imsize,y_up,Param,ipminfo);
%     figure;
%     imshow(ipm);
end
function y = get_lim(imsize,y_up,param,ipminfo)
    limx = [0;0;imsize(2);imsize(2)];limy = [y_up;imsize(1);y_up;imsize(1)];
    G = uv_get_xy2([limy,limx],param);
    realG1 = (G(:,1)-ipminfo(5))/ipminfo(4); % x
    realG2 = (G(:,2)-ipminfo(6))/ipminfo(3); % y
    k1 = (realG2(2)-realG2(1))/(realG1(2)-realG1(1));
    b1 = realG2(1) - k1*realG1(1);
    k2 = (realG2(3)-realG2(4))/(realG1(3)-realG1(4));
    b2 = realG2(3) - k2*realG1(3);
    x1 = [1:floor(realG1(2))];x2 = [floor(realG1(2))+1:floor(realG1(4))];
    x3 = [floor(realG1(4))+1:ipminfo(2)];
    y1 = k1*x1+b1;y3 = x3*k2+b2;y2 = zeros(1,size(x2,2));
    y = [y1,y2,y3];
end
function [ipminfo] = calc_ipminfo(x,y,param)
    III = [y,x];
    G = uv_get_xy2(III,param);
    ipmleft = min(G(:,1));
    ipmright = max(G(:,1));
    ipmtop = max(G(:,2));
    ipmbottom = min(G(:,2));
    ipmheight = 240;ipmwidth = 480;
    stepi = (ipmtop-ipmbottom)/ipmheight;
    stepj = (ipmright-ipmleft)/ipmwidth;
    ipminfo = [ipmheight,ipmwidth,stepi,stepj,ipmleft,ipmbottom];
end
%% able to compute matrix
function G = uv_get_xy2(P,Param)
    sita = Param(1);
    alpha = Param(2);
    beta = Param(3);
    m = Param(4);
    n = Param(5);
    h = Param(6);
    ux = P(:,1);
    vx = P(:,2);
    sizep = size(P);
    G = zeros(sizep(1),2);
    Y = h  *  cot(     sita   -   atan(    tan(alpha)   *   (1  -  2*ux  /  (m-1))   )      );
    X = sqrt(h*h+Y.*Y).*(tan(beta)*(2*vx/(n-1)-1)./sqrt(1+(tan(alpha)*(1-2*ux/(m-1))).*(tan(alpha)*(1-2*ux/(m-1)))));
    G(:,1) = X;G(:,2) = Y;
end
%% able to comput matrix
function P = xy_get_uv2(G,Param)
    sita = Param(1);
    alpha = Param(2);
    beta = Param(3);
    m = Param(4);
    n = Param(5);
    h = Param(6);
    X = G(:,1);
    Y = G(:,2);
    SIZEG = size(G);
    P = zeros(SIZEG(1),2);
    U = tan(sita-acot(Y/h));
    V = X./sqrt(h*h+Y.*Y).*sqrt(1+tan(sita-acot(Y/h)).*tan(sita-acot(Y/h)));
     u = (m-1)*(1-U/tan(alpha))/2;
     v = (n-1)*(1+V/tan(beta))/2;
    P(:,1) = u;
    P(:,2) = v;
end
function ipm = get_ipm(im,param,ipminfo)
    ipmheight = ipminfo(1);ipmwidth = ipminfo(2);
    stepi = ipminfo(3);stepj = ipminfo(4);
    ipmleft = ipminfo(5);ipmbottom = ipminfo(6);
    ipm = zeros(ipmheight,ipmwidth);
    [corx,cory] = find(~isnan(ipm));
    corx = corx*stepi+ipmbottom;
    cory = cory*stepj+ipmleft;
    cor = xy_get_uv2([cory,corx],param);
    coru = reshape(cor(:,2),[ipmheight,ipmwidth]);
    corv = reshape(cor(:,1),[ipmheight,ipmwidth]);
    ipm = interp2(double(im),coru,corv);ipm = uint8(ipm);
end
