using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;


namespace Messages
{
    public static class Dialogs
    {
        /// <summary>
        /// Словарь строк - элементов сообщений диалогов с пользователем.
        /// </summary>
        public static Dictionary<String, String> messages;

        /// <summary>
        /// Перечень фаз синтеза для валидации *.xml-файла.
        /// </summary>
        private static List<KeyValuePair<String, String>> synthesisPhases;

        /// <summary>
        /// Текущая фаза синтеза ГО.
        /// </summary>
        public static String currentSynthesisStep;

        /// <summary>
        /// Индекс нажатой кнопки в диалоге
        /// </summary>
        private static MessageBoxResult closeEvent;

        /// <summary>
        /// Отказ от сохранения
        /// </summary>
        public static Int32 exitSave;

        /// <summary>
        /// Номер текущего записываемого в базу данных элемента расписания
        /// </summary>
        public static Int32 elementCode;

        /// <summary>
        /// Сообщение о невозможности прдолжения синтеза графика
        /// </summary>
        public static void unableContinueSynthesis()
        {
            // @TODO: следующая за этой фаза!!!
            closeEvent = MessageBox.Show(messages["synthesisContinueFromPhase"] + messages[currentSynthesisStep] + messages[""], messages["captionMain"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Запрос на продолжение синтеза графика
        /// </summary>
        /// <returns>1 - продолжить синтез, -1 - начать синтез заново, 0 - отмена синтеза.</returns>
        public static SByte SynthesisPhaseQuery()
        { 
            SByte result = 0;
            // @TODO: следующая за этой фаза
            closeEvent = MessageBox.Show(messages["synthesisContinueFromPhase"] + messages[currentSynthesisStep] + messages["synthesisStartAgain"] + System.Environment.NewLine + messages["synthesisPhaseQuery"], messages["captionMain"], MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            switch (closeEvent)
            {
                case MessageBoxResult.Yes: result = 1;
                    break;
                case MessageBoxResult.No: result = -1;
                    break;
                case MessageBoxResult.Cancel: result = 0;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Сообщение об окончании стадии синтеза графика.
        /// </summary>
        /// <returns>True - продолжить синтез, false - прервать.</returns>
        public static Boolean EndStepQuery()
        {
            // @TODO: следующая за этой фаза
            closeEvent = MessageBox.Show(messages["synthesisContinueFromPhase"] + messages[currentSynthesisStep] + "?", messages["captionMain"], MessageBoxButton.YesNo, MessageBoxImage.Question);
            Logger.Output("EndStepQuery(): " + closeEvent.ToString() + " (Yes - продолжить синтез, No - прервать)", typeof(Dialogs).FullName);
            return closeEvent == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Инициализация по строкам из файла локализации.
        /// </summary>
        /// <param name="path">Путь к файлу со строками.</param>
        /// <returns>Удачно или нет произошло считывание.</returns>
        public static Boolean init(String path = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
                //System.Resources.ResourceManager.CreateFileBasedResourceManager()
            }

            if (path.Trim().Equals(String.Empty))
                throw new ArgumentException("Invalid filepath");

            messages = new Dictionary<String, String>()
                { 
                    {"trafficSchedule", ""},
                    {"fromNightRest", ""},
                    {"toMorningPeak", ""},
                    {"morningPeak", ""},
                    {"fromMorningPeak", ""},
                    {"dayNonpeak", ""},
                    {"toEveningPeak", ""},
                    {"eveningPeak", ""},
                    {"fromEveningPeak", ""},
                    {"toNigthRest", ""}
                };


            try
            {
                Logger.Output("Путь к файлу локализации: " + path, typeof(Dialogs).FullName);

                var doc = XDocument.Load(path);
                var nsp = doc.Root.Name.Namespace;
                var msg = doc.Descendants(nsp + "item");
                foreach (var node in msg)
                {
                    messages[node.Attribute("name").Value] = node.Attribute("value").Value;
                    //messages.Add(node.Attribute("name").Value, node.Attribute("value").Value);
                }

            }
            catch (System.IO.FileNotFoundException e)
            {
                Logger.Output(message: e.ToString(), source: typeof(Dialogs).FullName);
            }


            var result = (messages.ContainsValue("")); // есть пустые
            if (result)
            {
                var sb = new StringBuilder();
                var keys = messages.Where(msg => msg.Value.Equals("")).Select(msg => msg.Key);
                foreach (var item in keys)
                    sb.AppendFormat("{0}, ", item);
                Logger.Output(sb.ToString(0, sb.Length - 2), typeof(Dialogs).FullName);
            }
            return !result;
            // @NOTE: "Наука требует интимного размышления и общественного обсуждения." -- проф. Баранов Л. А. (07-10-2013 18:43)

            

            /*
            // Public Const StepGrOb = "графика оборота"
            // MasStep(1) = StepGrOb
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["trafficSchedule"]);
            // Public Const StepGrPik1 = "утреннего пика"
            // MasStep(2) = StepNightMorn
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["fromNightRest"]);
            // Public Const StepNightMorn = "выхода из ночного отстоя"
            // MasStep(3) = StepMorn
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["toMorningPeak"]);
            // Public Const StepMorn = "входа в утренний пик"
            // MasStep(4) = StepGrPik1
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["morningPeak"]);
            // Public Const StepAfterPik1 = "выхода из утреннего пика"
            // MasStep(5) = StepAfterPik1
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["fromMorningPeak"]);
            // Public Const StepDay = "дневного непика"
            // MasStep(6) = StepDay 'StepToPik2
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["dayNonpeak"]);
            // Public Const StepToPik2 = "входа в вечерний пик"
            // MasStep(7) = StepToPik2
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["toEveningPeak"]);
            // Public Const StepPik2 = "вечернего пика"
            // MasStep(8) = StepPik2
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["eveningPeak"]);
            // Public Const StepAfterPik2 = "выхода из вечернего пика"
            // MasStep(9) = StepAfterPik2
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["fromEveningPeak"]);
            // Public Const StepToNight = "ухода на ночную расстановку" 
            // MasStep(10) = StepToNight
            mdlData.masSynthesisStep.Add(Messages.Dialogs.messages["toNightRest"]);
             */
        }
    }
}
