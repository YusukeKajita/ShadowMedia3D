   M S B A M L   `   `   `  ���� I  E3DTools, Version=1.0.2614.20437, Culture=neutral, PublicKeyToken=null4'clr-namespace:_3DTools;assembly=3DTools_3DTools   ShadowKinectSensor3.0#   ShadowKinectSensor.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     9tools'clr-namespace:_3DTools;assembly=3DTools     -     � Title$  
MainWindow��$	��534��$	��893��
���   �
���   �
������� � 
ResizeMode$ NoResize=�$��5q� � WindowStyle$
 None=� � AllowsTransparency .  J� Opacity$
 0.85Q� � 	Resources  �� MergedDictionaries �� Source��  �� $ sphere.xaml?�%    .��� ����  �� EndPoint$ 0.5,1&�	 �� 
StartPoint$	 0.5,0&�.��� 
 � Color$
 Black��� $
 	#FF610404�� � Offset$ 1Q� � RowDefinitions �� $��25 ��� $��* � � ColumnDefinitions �� $��5* ��� $��6* �.+���  �� Orientation$ 
Horizontal=�$��RightToLeft=�$��1��.+��� -   $
�� × ��   WindowButtonTemplate#��[   � ToolTip$ 正常終了します���� -   $�� ＿ ��#��[  $ 最小化します���� $��操作��$��1��$��5q�.���� .@��� $��設定��.��� .+��  � Orientation$ 
Horizontal=�.+��� $��送信データ型選択��$��5q�.��� $ Vertical=�.+�	� -    	�Name$ DepthDataButton��$��	DepthData��$) #送信先：CIPC 受信先：Kinect��	� -   $ SkeletonDataButton��$��SkeletonData��$) #送信先：CIPC 受信先：Kinect��	� -   $ OptitrackButton��$��
OptiTtrack��$' !送信先：CIPC 受信先：Opti��	� -   $, &RadioButton_ShadowKinectSensorTerminal��$ ��ShadowKinectSensorTerminal��$) #送信先：SKST 受信先：Kinect���� -    ��Name$ TextBlock_State������ �� $��	受信先��.��� .+�� .+��� -	    ��Name$% GroupBox_ReceiveFrom_CIPCServer��$��5q���	� $��
CIPCServer�� 	� 	GroupName$ ReceiveFrom��.���� .+��� $��RemoteIP��.���� -
    ��Name$ TextBox_remoteIP��$��	127.0.0.1���� $��
RemotePort��.���� -   $ TextBox_remotePort��$��12000��$	��Top=��� $��myPort��.���� -   $ TextBox_myPort�� �� TextWrapping$
 Wrap=�$
��4001���� $ 
Horizontal=�.+��� -    ��Name$ CIPC_lunchbutton_Receive��$��5q�$��
CIPC接続���� -   $ CIPC_closebutton_Receive��$��5q�$��
CIPC切断��{�.  �� -   $! GroupBox_ReceiveFrom_Kinect��$��5q���	� -   $$ RadioButton_ReceiveFrom_Kinect��$��Kinect��$ ReceiveFrom�� d� 	IsChecked$
 TrueY�.���� .+��� $ 
Horizontal=�.+��� $��index : ���� -   $ TextBox_Kinect_index��$��0���� $ 
Horizontal=�.+��� -   $ KinectLunchButton��$�� 　起動　 ��$��5q��� -   $ KinectStopButton��$�� 　終了　 ��$��5q�{�.  �� -   $ TextBlock_KinectStatus��$
��Wrap=�$��	状態：���� -   $$ GroupBox_ReceiveFrom_Optitrack��$��5q���	� -   $' !RadioButton_ReceiveFrom_Optitrack��$��	Optitrack��$ ReceiveFrom��.���� $
��Left=�.+��� $��ServerAddress��.���� -   $ opt_ServerAddress��$
 Wrap=�$��	127.0.0.1���� $��localAddress��.���� -   $ opt_localAdress��$��	127.0.0.1���� $ 
Horizontal=�.+��� -   $ OptReceiveButton��$�� 受信開始 ��$��5q��� -   $ OptCloseReceiveButton��$�� 受信終了 ��$��5q�{�.  �� -   $ 	OptStatus��$
��Wrap=�$��	状態：���� $��	送信先��.��� .+��� -   $  GroupBox_Sendto_CIPCServer������ $��
CIPCServer��.���� .+��� $��RemoteIP��.���� -   $ tbx_remoteIP��$��	127.0.0.1���� $��
RemotePort��.���� -   $ tbx_remotePort��$��12000��$	��Top=��� $��myPort��.���� -   $ 
tbx_myPort��$
 Wrap=�$
��4050���� $��SendFPS��$ 目標値　目安��.���� -    $ tbx_fps��$
 Wrap=�$��30���� $ 
Horizontal=�.+��� -!   $ CIPC_lunchbutton��$��
CIPC接続��$��5q��� -"   $ CIPC_closebutton��$��
CIPC切断��$��5q�{�.  �� -#   $ GroupBox_Sendto_SKST������ $ ��ShadowKinectSensorTerminal��.���� .+��� $��RemoteIP��.���� -$   $ TextBox_SKST_remoteIP��$��	127.0.0.1���� $ 
Horizontal=�.+��� $��	UDP接続��.���� .+��� $��
RemotePort��.���� -%   $ TextBox_SKST_remotePort��$
��4020���� $��BindPort��.���� -&   $ TextBox_SKST_BindPort��$
��4051���� $��	TCP接続��.���� .+��� $��
RemotePort��.���� -'   $! TextBox_SKST_TCP_remotePort��$��14020���� $��SendFPS��$ 目標値　目安��.���� -(   $ tbx_SKST_fps��$
 Wrap=�$��30���� $ 
Horizontal=�.+��� -)   $ Button_Sendto_SKST_Start��$�� 接　続 ��$��5q��� -*   $ Button_Sendto_SKST_Stop��$�� 切　断 ��$��5q�{�.  �� $��1��$��1��$��5q�$��表示��.���� -+    ��Name$ TabControl_View��.@��� $��標準��.��� $��1��$��1��.+��� $��深度��.��� $	��160��$	��120��.+��� -,    ��Name$ DepthImageBox���� $��	カラー��.��� $	��160��$	��120��.+��� --   $ ColorImageBox���� $��ボーン　&　影画像��$��1��.��� $	��160��$	��120��.+��� -.   $ PlayerShadowBox��� -/    �Name$ SkeltonGrid������  �� ElementName$ PlayerShadowBox�� �� Path$ Width����� $ PlayerShadowBox��$ Height��� $��	三次元��.��� .+��� -0   $ TabControl_Modelview��.@��� $��取得モデル��.���� $��3DView1��.��� $	��320��$	��240��.+�'� -1    '�Name$ Viewport3D_1�� '� Camera =� -2   Camera_1��  � Position$  0,0,-51�! � UpDirection$! 0,1,05�" � LookDirection$" 0,0,-15�x�g� # g� Content# t� $ �� Color$$ White��% t� 	Direction$% 0,-0.5,15��� $��3Dモデル��.���� $��3DView2��.��� $	��320��$	��240��.+�'� -3   $ Viewport3D_2�� =� -4   Camera_2��$  0,0,-51�$! 0,1,05�$" 0,0,-15�x�g� # t� $$ White��$% 0,-0.5,15��� $��送信��.��� $	��320��$	��240��.+��� $ ��ShadowKinectSensorTerminal��.���� .+��� $��remoteIP��.���� ���� $ TextBox_SKST_remoteIP��$
 Text��� $��
remotePort��.���� ���� $! TextBox_SKST_TCP_remotePort��$
 Text��� $��　送　信　��{��� $, &RadioButton_ShadowKinectSensorTerminal��$ 	IsChecked��� $��3Dmodel取得��$	��100��.���� .+��� -5   $��取得���� .+��� .+��� $ 
Horizontal=�.+��� $��Z Max : ���� ���� $ Slider_Cube_Zmax��$ Value��� -6   & ��Name$& Slider_Cube_Zmax��' � Value$' 4Q�( � Minimum$( 0Q�) � Maximum$) 10Q�* �� AutoToolTipPlacement$* TopLeft=��� .+��� $ 
Horizontal=�.+��� $��Z Min : ���� ���� $ Slider_Cube_Zmin��$ Value��� -7   $& Slider_Cube_Zmin��$' 3Q�$( 0Q�$) 10Q�$* TopLeft=��� .+��� $ 
Horizontal=�.+��� $��X Max : ���� ���� $ Slider_Cube_Xmax��$ Value��� -8   $& Slider_Cube_Xmax��$	' 1.5Q�$( 0Q�$) 5Q�$* TopLeft=��� .+��� $ 
Horizontal=�.+��� $��X Min : ���� ���� $ Slider_Cube_Xmin��$ Value��� -9   $& Slider_Cube_Xmin��$
' -1.5Q�$( -5Q�$) 0Q�$* TopLeft=��� .+��� $ 
Horizontal=�.+��� $��Y Max : ���� ���� $ Slider_Cube_Ymax��$ Value��� -:   $& Slider_Cube_Ymax��$' 1Q�$( 0Q�$) 3Q�$* TopLeft=��� .+��� $ 
Horizontal=�.+��� $��Y Min : ���� ���� $ Slider_Cube_Ymin��$ Value��� -;   $& Slider_Cube_Ymin��$' -1Q�$( -3Q�$) 0Q�$* TopLeft=��� .+��� $��処理操作��.���� .+��� $ 
Horizontal=�.+��� $��	処理率���� -<   $& Slider_Frame��$	��150��$( 1Q�$' 2Q�+ �� IsSnapToTickEnabled+ . , � LargeChange$, 5Q�- � SmallChange$- 1Q�$* TopLeft=��� $ 
Horizontal=�.+��� $��	解像度���� -=   $& 
Slider_DPI��$	��150��$( 5Q�$) 25Q�$' 10Q�+ . $, 5Q�$- 5Q�$* TopLeft=�. �� TickFrequency$. 1Q��� $��3Dモデル保存��.���� .+��� $ 
Horizontal=�$��Hidden=�.+��� ->   $ TextBox_Filename��$	��100���� -?   $$ Button_3Dmodel_Choise_Filename��$��
 選  択 ��$��0,0,10,0q��� -@   $ Button_3Dmodel_Save��$��
 保  存 ���� $��視点操作��.���� .+��� $ 
Horizontal=�.+��� $��ＸＺ距離���� -A   $& Slider_Length��$	��150��$) 20Q�$( 1Q�$' 10Q�+ . $	. 0.1Q�$* BottomRight=��� $ 
Horizontal=�.+��� $��Ｙ軸回転���� -B   $& Slider_Rotate��$* BottomRight=�$	��150��$	) 360Q�$( 0Q�$	' 270Q�+ . �� $ 
Horizontal=�.+��� $��Ｙ軸高さ���� -C   $& Slider_Height��$* BottomRight=�$	��150��$) 20Q�$	( -20Q�$' 0Q�+ . $	. 0.5Q��� $ 
Horizontal=�.+��� $��視中心Ｘ���� -D   $& Slider_X��$* BottomRight=�$	��150��$) 20Q�$	( -20Q�$' 0Q�+ . $	. 0.5Q��� $ 
Horizontal=�.+��� $��視中心Ｚ���� -E   $& Slider_Z��$* BottomRight=�$	��150��$) 20Q�$	( -20Q�$' 5Q�+ . $	. 0.5Q��� $!��キャリブレーション��.��� .+��� $��情報��$	��200��.���� .+��� $ 
Horizontal=�.+��� $��相対位置X : ���� -F   $ TextBlock_Callib_X���� $ 
Horizontal=�.+��� $��相対位置Y : ���� -G   $ TextBlock_Callib_Y���� $ 
Horizontal=�.+��� $��相対位置Z : ���� -H   $ TextBlock_Callib_Z���� $ 
Horizontal=�.+��� $��回転角度x : ���� -I   $ TextBlock_Callib_rx���� $ 
Horizontal=�.+��� $��回転角度y : ���� -J   $ TextBlock_Callib_ry���� $ 
Horizontal=�.+��� $��回転角度z : ���� -K   $ TextBlock_Callib_rz���� $ 
Horizontal=�.+��� $��原点までの距離 : ���� -L   $ TextBlock_Callib_D���� $��Slot1��.���� .+��� -M   $ Button_Callib_Save1��$��保存���� -N   $ Button_Callib_Load1��$��読込���� $��Slot2��.���� .+��� -O   $ Button_Callib_Save2��$��保存���� -P   $ Button_Callib_Load2��$��読込���� $��Slot3��.���� .+��� -Q   $ Button_Callib_Save3��$��保存���� -R   $ Button_Callib_Load3��$��読込���� $��検査��$	��300��.���� .+��� $��燈火位置���� $ 
Horizontal=�.+��� $��
相対x : ���� -S   $  TextBlock_Callib_Test_x_re���� $ 
Horizontal=�.+��� $��
相対y : ���� -T   $  TextBlock_Callib_Test_y_re���� $ 
Horizontal=�.+��� $��
相対z : ���� -U   $  TextBlock_Callib_Test_z_re���� $ 
Horizontal=�.+��� $��
絶対x : ���� -V   $  TextBlock_Callib_Test_x_ab���� $��
絶対y : ���� -W   $  TextBlock_Callib_Test_y_ab���� $��
絶対z : ���� -X   $  TextBlock_Callib_Test_z_ab���� -Y   $ Button_Callib_test��$��検  査���� $'��!試験キャリブレーション��$	��200��.���� .+��� -Z   $ Button_Callib_Zero��$
��Zero���� -[   $ Button_Callib_X��$��X���� -\   $ Button_Callib_Z��$��Z���� -]   $ Button_Callib_Calc��$��計算���� $��前後判定��.��� .+��� $��現在の向き��.��� $	��120��$	��160��.+��� -^   / ��Name$/ Rectangle_Judge_Allow��$��70,20,70,20q�0 J� RenderTransformOrigin$0 0.5,0.5&�$��3��1 �� RadiusX$1 3��2 �� RadiusY$2 3��
���   �3 �� StrokeDashOffset$3 -4Q����� $ 0.5,1&�$	 0.5,0&�.��� $
 White��� $
 White��$ 1Q�� $	
 Red��$ 0.513Q�� $
 	#FF5443FF��$ 0.504Q��� $��更新��.��� $	��120��$	��160��.+��� -_   $ TextBlock_Judge_Update��$��72(��� $��向き��.��� $	��120��$	��160��.+��� -`   $ TextBlock_Judge_Direction��$��72(��� -a   $ Button_Judge_LookFront��$��今前向いています���� -b   $ Title_TextBlock��$
��Wrap=�$��2��4 J� IsHitTestVisible4 .  