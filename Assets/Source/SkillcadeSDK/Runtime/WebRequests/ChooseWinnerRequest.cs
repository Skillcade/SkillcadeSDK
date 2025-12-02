namespace SkillcadeSDK.WebRequests
{
#if UNITY_SERVER
    public class ChooseWinnerRequest
    {
        public string WinnerId { get; set; }
    }
#endif
}