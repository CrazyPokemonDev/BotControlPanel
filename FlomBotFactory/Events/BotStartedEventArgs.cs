namespace FlomBotFactory.Events
{
    public class BotStartedEventArgs
    {
        public FlomBot Bot;

        public BotStartedEventArgs(FlomBot bot)
        {
            this.Bot = bot;
        }
    }
}
