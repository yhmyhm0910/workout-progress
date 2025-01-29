UPDATE users
SET 
    refresh_token = COALESCE(@RefreshToken, refresh_token)
WHERE userID = @UserID;
