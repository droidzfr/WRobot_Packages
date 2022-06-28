using robotManager.Helpful;
using System.Threading;
using robotManager.Products;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net;

// alerts sounds 
// whisper: http://soundbible.com/2154-Text-Message-Alert-1.html
// death: http://www.wowhead.com/sound=2944/humanmaleplayerdeath

public class Main : wManager.Plugin.IPlugin
{
    private bool _isLaunched;

    // deathAlert 
    private int lastPlayerAliveTime;
    private int lastPlayerDeadTime;

    // whisperAlert
    private Dictionary<string, int> PlayersByWhisper = new Dictionary<string, int>();
    private Dictionary<string, string> AlertByPath = new Dictionary<string, string>();
    private int lastReadMessageID;
    private readonly List<ChatTypeId> whisperChatIDs = new List<ChatTypeId> { ChatTypeId.WHISPER, ChatTypeId.BN_WHISPER };

    System.Media.SoundPlayer notificationPlayer = new System.Media.SoundPlayer();

    SmtpClient smtp = new SmtpClient();

    public void Initialize()
    {
        Logging.Write("[RoboAlert] Started.");
        _isLaunched = true;


        AlertByPath.Add("whisper", _settings.whisperAlertSoundFilePath);
        AlertByPath.Add("death", _settings.deathAlertSoundFilePath);

        notificationPlayer = new System.Media.SoundPlayer();

        initializeEmail();

        lastPlayerAliveTime = DateTime.Now.Millisecond;
        lastPlayerDeadTime = lastPlayerAliveTime;
        lastReadMessageID = wManager.Wow.Helpers.Chat.Messages.Count - 1;

        doStuffLoop();

    }
    public void Dispose()
    {
        notificationPlayer.Dispose();
        lastPlayerAliveTime = 0;
        lastPlayerDeadTime = 0;
        _isLaunched = false;
        Logging.Write("[RoboAlert] Disposed.");
    }
    public void Settings()
    {
        _settings.ToForm();
        _settings.Save();
    }
    private roboAlertSettings _settings
    {
        get
        {
            try
            {
                if (roboAlertSettings.CurrentSetting == null)
                    roboAlertSettings.Load();
                return roboAlertSettings.CurrentSetting;
            }
            catch
            {
            }
            return new roboAlertSettings();
        }
        set
        {
            try
            {
                roboAlertSettings.CurrentSetting = value;
            }
            catch
            {
            }
        }
    }

    public void doStuffLoop()
    {

        while (Products.IsStarted && _isLaunched)
        {

            if (!Products.InPause)
            {
                if (_settings.deathAlertEnabled)
                    deathAlert();

                if (_settings.whisperAlertEnabled)
                    whisperAlert();

                Thread.Sleep(3000);
            }
        }

    }

    public void deathAlert()
    {

        bool isPlayerAlive = wManager.Wow.ObjectManager.ObjectManager.Me.IsAlive;
        bool isPlayerDead = wManager.Wow.ObjectManager.ObjectManager.Me.IsDead;

        if (isPlayerAlive)
        {

            lastPlayerAliveTime = DateTime.Now.Millisecond;

        }
        if (isPlayerDead)
        {
            // this means the player recently died.
            if (lastPlayerDeadTime < lastPlayerAliveTime)
            {
                lastPlayerDeadTime = DateTime.Now.Millisecond;

                Logging.Write("[RoboAlert] Character Player Died!");
                if (_settings.soundOnDeathEnabled)
                    playSound("death");
                if (_settings.emailOnDeathEnabled)
                    sendEmail("death");


            }


        }
    }
    public void whisperAlert()
    {
        var msgs = wManager.Wow.Helpers.Chat.Messages;

        while (lastReadMessageID + 1 <= msgs.Count - 1)
        {
            lastReadMessageID++;
            // if the Message in Chat was a whisper
            if (whisperChatIDs.Contains(msgs[lastReadMessageID].Channel))
            {


                // log to file.
                Logging.Write("[RoboAlert] New whisper " + msgs[lastReadMessageID]);

                if (Products.IsStarted && !Products.InPause && _isLaunched)
                {

                    if (_settings.soundOnWhisperEnabled)
                        playSound("whisper");
                    if (_settings.emailOnWhisperEnabled)
                        sendEmail("whisper");



                    // Increment our count for players # of received message.
                    // If user exists in Dictionary, increment count of recieved messages.
                    if (PlayersByWhisper.ContainsKey(msgs[lastReadMessageID].UserName))
                    {
                        PlayersByWhisper[msgs[lastReadMessageID].UserName]++;
                    }
                    else
                    {
                        // Create new entry in dictionary, and set value to 1.
                        PlayersByWhisper.Add(msgs[lastReadMessageID].UserName, 1);
                    }
                }



            }
        }
    }

    private void initializeEmail() {

        smtp.Host = "smtp.gmail.com";
        smtp.Port = 587;
        smtp.UseDefaultCredentials = false;
        smtp.EnableSsl = true;

        NetworkCredential nc = new NetworkCredential(_settings.gmailEmailAddress, _settings.gmailPassword);
        smtp.Credentials = nc;
    }

    // Must be configured on email account before this will work.
    //https://accounts.google.com/DisplayUnlockCaptcha
    //https://myaccount.google.com/lesssecureapps
    private void sendEmail(string alert) {

        Logging.Write("[RoboAlert] Sending email....");

        MailMessage msg = new MailMessage();
        msg.Subject = ("[RoboAlert] Notification " + alert);
        msg.From = new MailAddress(_settings.gmailEmailAddress);
        msg.Body = "You are recieving this message because you have configured roboAlert to send messages when the following alert occurs: " + alert;
        msg.To.Add(new MailAddress(_settings.gmailEmailAddress));     
        try
        {
            smtp.Send(msg);
        }
        catch (Exception e)
        {
            Logging.Write("" + e);
            //https://accounts.google.com/DisplayUnlockCaptcha
            //https://myaccount.google.com/lesssecureapps
        }
        msg.Dispose();

    }

    // must be a wav.
    //http://www.stenographsolutions.com/kb_upload/image/Audio/Audacity/audacity.gif
    //notificationPlayer.SoundLocation = @"C:\Fast_Applications\WRobot\WRobot\Plugins\roboAlert\whisperAlert.wav";
    private void playSound(string alertType)
    {
        string path = AlertByPath[alertType];

        try
        {
            if (File.Exists(path))
            {
                notificationPlayer.SoundLocation = AlertByPath[alertType];
                notificationPlayer.Load();
                notificationPlayer.Play();
            }
            else // path doesn't exist. so play default sounds
            {

                Logging.Write("[RoboAlert] File does not exist for alert " + alertType + " at path: " + path);
                Logging.Write("[RoboAlert] Playing default alert sound.");

                if (alertType == "death")
                {
                    path = Path.Combine(Environment.CurrentDirectory, @"Plugins\RoboAlert\", "deathAlert.wav");
                }
                else if (alertType == "whisper")
                {
                    path = Path.Combine(Environment.CurrentDirectory, @"Plugins\RoboAlert\", "whisperAlert.wav");
                }

                notificationPlayer.SoundLocation = path;
                notificationPlayer.Load();
                notificationPlayer.Play();

            }
        }
        catch (Exception e)
        {
            Logging.Write("[RoboAlert] problem playing notification sound for alert type: " + alertType + ". " + e);
        }

    }

    [Serializable]
    public class roboAlertSettings : Settings
    {
        public roboAlertSettings()
        {
            whisperAlertSoundFilePath = Path.Combine(Environment.CurrentDirectory, @"Plugins\RoboAlert\", "whisperAlert.wav");
            deathAlertSoundFilePath = Path.Combine(Environment.CurrentDirectory, @"Plugins\RoboAlert\", "deathAlert.wav");

            deathAlertEnabled = true;
            whisperAlertEnabled = true;

            soundOnDeathEnabled = true;
            soundOnWhisperEnabled = true;
            emailOnDeathEnabled = false;
            emailOnWhisperEnabled = false;

            gmailEmailAddress = "";
            gmailPassword = "";
        }

        public bool deathAlertEnabled { get; set; }
        public bool whisperAlertEnabled { get; set; }

        public bool soundOnDeathEnabled { get; set; }
        public bool soundOnWhisperEnabled { get; set; }
        public bool emailOnDeathEnabled { get; set; }
        public bool emailOnWhisperEnabled { get; set; }

        public string gmailEmailAddress { get; set; }
        public string gmailPassword { get; set; }

        public string whisperAlertSoundFilePath { get; set; }
        public string deathAlertSoundFilePath { get; set; }

        public static roboAlertSettings CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("RoboAlert", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("RoboAlert > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("RoboAlert", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<roboAlertSettings>(AdviserFilePathAndName("RoboAlert",
                                                                      ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new roboAlertSettings();
            }
            catch (Exception e)
            {
                Logging.WriteError("RoboAlert > Load(): " + e);
            }
            return false;
        }
    }

}