namespace FlomBotFactory.Events
{
    public class BotStoppedEventArgs
    {
        public FlomBot Bot;

        public BotStoppedEventArgs(FlomBot bot)
        {
            this.Bot = bot;
        }
    }
}
