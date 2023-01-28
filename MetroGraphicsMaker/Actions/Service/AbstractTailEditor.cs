using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

using Core;

namespace Actions
{
    //Необходимо проверить нужен ли этот класс и возможно это хорошая идея: выделить операции для хвостов в отдельную иерархию.
    class AbstractTailEditor
    {
        int MemoryDifferenceBetweenTrainPath;
        int MemoryStartTimeTail;
        List<Point> MemoryLogicalTailPoints = null;

        AbstractTail EditLogicalTail;
        TrainPath EditLogicalTrainPath;

        int DeltaTime;

        public AbstractTailEditor(AbstractTail _EditLogicalTail, TrainPath _EditLogicalTrainPath)
        {
            EditLogicalTail = _EditLogicalTail;

            MemoryLogicalTailPoints = new List<Point>(EditLogicalTail.ViewAbstractTail.TailPoints.Count);
            foreach (Point thPoint in EditLogicalTail.ViewAbstractTail.TailPoints)
            {
                MemoryLogicalTailPoints.Add(thPoint);
            }
            EditLogicalTrainPath = _EditLogicalTrainPath;
            MemoryStartTimeTail = EditLogicalTail.TimeBeginTail;
        }

        public Boolean Check(int _DeltaTime)
        {
            DeltaTime = _DeltaTime;
            if (EditLogicalTail.LeftLogicalTrainPath != EditLogicalTrainPath)
            {
                LinkedTail thLinkedTail;
                if (EditLogicalTail.NameTail == AbstractTailGiver.NamesTails.LinkedTail || EditLogicalTail.NameTail == AbstractTailGiver.NamesTails.LinkedTailCustom)
                {
                    thLinkedTail = (LinkedTail)EditLogicalTail;
                    if (thLinkedTail.RightLogicalTrainPath != EditLogicalTrainPath) return false;
                }
            }
            return true;
        }

        public void Do()
        {


            if (EditLogicalTail.NameTail != AbstractTailGiver.NamesTails.LinkedTail && EditLogicalTail.NameTail != AbstractTailGiver.NamesTails.LinkedTailCustom)
            {
                EditLogicalTail.TimeBeginTail = MemoryStartTimeTail + DeltaTime;
            }    
            else
            {
                double СoefficientСhanges;
                if (EditLogicalTail.LeftLogicalTrainPath == EditLogicalTrainPath)
                {
                    EditLogicalTail.TimeBeginTail = MemoryStartTimeTail + DeltaTime;
                    СoefficientСhanges = (MemoryLogicalTailPoints.Last().X - (MemoryLogicalTailPoints.First().X + DeltaTime)) / (MemoryLogicalTailPoints.Last().X - MemoryLogicalTailPoints.First().X);
                }
                else
                {
                    СoefficientСhanges = (MemoryLogicalTailPoints.Last().X - (MemoryLogicalTailPoints.First().X - DeltaTime)) / (MemoryLogicalTailPoints.Last().X - MemoryLogicalTailPoints.First().X);
                }
                EditLogicalTail.ViewAbstractTail.TailPoints.Clear();
                foreach (Point thPoint in MemoryLogicalTailPoints)
                {
                    EditLogicalTail.ViewAbstractTail.TailPoints.Add(new Point((int)thPoint.X * СoefficientСhanges, thPoint.Y));
                }
            }
            EditLogicalTail.ViewAbstractTail.InvalidateVisual();
        }

        public void Undo()
        {
            EditLogicalTail.TimeBeginTail = MemoryStartTimeTail;
            if (EditLogicalTail.NameTail == AbstractTailGiver.NamesTails.LinkedTail || EditLogicalTail.NameTail == AbstractTailGiver.NamesTails.LinkedTailCustom)
            {
                EditLogicalTail.ViewAbstractTail.TailPoints.Clear();
                foreach (Point thPoint in MemoryLogicalTailPoints)
                {
                    EditLogicalTail.ViewAbstractTail.TailPoints.Add(new Point(thPoint.X, thPoint.Y));
                }
            }
            EditLogicalTail.ViewAbstractTail.InvalidateVisual();
        }
    }
}
