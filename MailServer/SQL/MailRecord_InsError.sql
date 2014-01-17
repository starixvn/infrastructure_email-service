﻿USE [bodb_Maintenance]
GO
/****** Object:  StoredProcedure [dbo].[MailRecorder_InsError]    Script Date: 7/10/2013 9:53:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[MailRecorder_InsError]		
	@MailID bigint,	
	@AppID int = 0,	
	@Message varchar(1000) = '',
	@StackTrace varchar(2000) = '',	
	@ErrorTime datetime = NULL,
	@ErrorID int = 0 output,
	@ErrCode int = 0 Output,
	@ErrMsg nvarchar(200) = '' Output
AS
/*
Created: 07/10/2013 @Ricky
Task : Insert new mail error
DB: DB06.bodb_Maintenance
Original:
	
Param's Explanation:

*/
SET NOCOUNT ON

BEGIN
	DECLARE @SettingName nvarchar(250)
	SET  @SettingName = (SELECT TOP 1 s.SettingName
							FROM Mail_Setting AS s INNER JOIN
							Mail_Config AS c ON c.AppID = @AppID and c.SettingID = s.SettingID
							)

	INSERT INTO dbo.Mail_Error([MailID], [AppID], [Message], [StackTrace], [SettingName], [ErrorTime])
	VALUES (@MailID, @AppID, @Message , @StackTrace, @SettingName, @ErrorTime)

	IF @@Error <> 0
	BEGIN			
		SELECT @ErrMsg = 'Error! Error when inserting mail error;', @ErrCode = 101			
		RETURN
	END
	SELECT @ErrorID = @@IDENTITY
END

