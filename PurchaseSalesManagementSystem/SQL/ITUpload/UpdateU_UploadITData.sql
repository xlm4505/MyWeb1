UPDATE U_UploadITData SET StatusFlag = '1'  WHERE StatusFlag = '0' AND ClientAddress = @IpAddress;
