a = NET.addAssembly('/../NVStereoTest.dll');
b = NVStereoTest.WinForm();
b.InitializeComponent();
b.InitializeDevice();
b.LoadSurfaces();

L = NET.createArray('System.String',2);
L(1) = 'L1.png';
L(2) = 'L2.png';

R = NET.createArray('System.String',2);
R(1) = 'R1.png';
R(2) = 'R2.png';

L_list = NET.createGeneric('System.Collections.Generic.List',{'System.String'},2);
AddRange(L_list,L);

R_list = NET.createGeneric('System.Collections.Generic.List',{'System.String'},2);
AddRange(R_list,R);


b.LoadSurfacesImg(L_list,R_list);
b.Set3D(0,0);
b.Show();
pause;
tic;
b.LoadSurfaces();
b.Set3D(1,1);
b.Show();
toc;
pause;