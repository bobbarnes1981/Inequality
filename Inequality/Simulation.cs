using System;
using System.Drawing;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;

namespace Inequality
{
    class Simulation
    {
        private int m_width = 640;
        private int m_height = 480;

        private Surface m_video;

        private Person[] m_people;

        private int m_numberOfPeople = 40;

        private int m_startingMoney = 100;

        private Random m_random;

        private int m_elapsed;

        private int m_delay = 10;

        private int m_cycles = 0;

        private History m_history;

        private int m_scale = 4;

        private int m_peakDelay = 5;

        private int m_peakSpeed = 2;

        private int[] m_peakValues;

        private int[] m_peakDelays;

        private bool m_allowNegativeMoney = false;

        private int m_maxValue;

        private int m_numberOfGroups = 20;

        private int m_sizeOfGroups;

        private int[] m_groups;

        public Simulation()
        {
            m_video = Video.SetVideoMode(640, 480, false, false, false);

            m_people = new Person[m_numberOfPeople];

            m_random = new Random();

            // set up peaks

            m_peakValues = new int[m_numberOfPeople];
            m_peakDelays = new int[m_numberOfPeople];

            // set up people

            for (int i = 0; i < m_numberOfPeople; i++)
            {
                // random colour

                int r = m_random.Next(0, 256);
                int g = m_random.Next(0, 256);
                int b = m_random.Next(0, 256);

                m_people[i] = new Person(Color.FromArgb(r,g,b), m_startingMoney);

                // initialise peaks

                m_peakValues[i] = 0;
                m_peakDelays[i] = 0;
            }

            m_elapsed = 0;

            // init history

            m_history = new History(m_numberOfPeople, m_width);

            // init groups

            m_maxValue = m_scale*m_startingMoney;

            m_sizeOfGroups = m_maxValue/m_numberOfGroups;

            m_groups = new int[m_numberOfGroups];

            // connect events

            Events.Quit += Events_Quit;
            Events.Tick += Events_Tick;
        }

        private void Events_Tick(object sender, TickEventArgs e)
        {
            m_video.Fill(Color.Black);

            m_elapsed += e.TicksElapsed;
            if (m_elapsed > m_delay)
            {
                m_elapsed -= m_delay;

                // simulate

                simulate();

                // do updates after all simulation is complete

                for (int groupId = 0; groupId < m_numberOfGroups; groupId++)
                {
                    m_groups[groupId] = 0;
                }

                for (int personId = 0; personId < m_numberOfPeople; personId++)
                {
                    // update peaks

                    updatePeak(personId);

                    // update history for person

                    m_history.Push(personId, m_people[personId].Money);

                    // update groups

                    int groupId = m_people[personId].Money/m_sizeOfGroups;

                    if (m_groups.Length > groupId)
                    {
                        m_groups[groupId] += 1;
                    }
                }
            }

            SdlDotNet.Graphics.Font font = new SdlDotNet.Graphics.Font("C:\\Windows\\Fonts\\ARIAL.TTF", 12);

            // draw bars

            int barWidth = m_width / m_numberOfPeople;
            int barScale = m_height / (m_startingMoney * m_scale);

            for (int personId = 0; personId < m_numberOfPeople; personId++)
            {
                // draw bar

                m_video.Draw(new Box(
                    (short)(barWidth * personId), 0,
                    (short)((barWidth * personId) + barWidth - 1), (short)(barScale * m_people[personId].Money)),
                    m_people[personId].Color, false, true);

                // draw value

                m_video.Blit(font.Render(m_people[personId].Money.ToString(), Color.White).CreateRotatedSurface(270),
                    new Point((barWidth * personId), 10));

                // draw peak hold

                m_video.Draw(new Line(
                    (short)(barWidth * personId), (short)(barScale * m_peakValues[personId]),
                    (short)((barWidth * personId) + barWidth - 1), (short)(barScale * m_peakValues[personId])
                    ), Color.White, false, true);

                // draw lines

                for (int j = 0; j < m_width; j++)
                {
                    m_video.Draw(new Point(j, m_height - (barScale * m_history.Get(personId, j))), m_people[personId].Color);
                }
            }

            // draw distribution

            int groupWidth = m_width/m_numberOfGroups;
            int groupScale = m_height/m_numberOfPeople;

            short previousX = 0;
            short previousY = (short)m_height;
            short currentX;
            short currentY;
            for (int groupId = 0; groupId < m_numberOfGroups; groupId++)
            {
                currentX = (short)((groupWidth*groupId)+(groupWidth/2));
                currentY = (short)(m_height - (groupScale*m_groups[groupId]));
                m_video.Draw(new Line(
                    previousX, previousY,
                    currentX, currentY
                    ), Color.White, false, true);

                m_video.Blit(font.Render(m_groups[groupId].ToString(), Color.White),
                    new Point(currentX - 6, currentY - 24 - (m_groups[groupId] % 2 == 0 ? 0 : groupScale)));

                previousX = currentX;
                previousY = currentY;
            }
            m_video.Draw(new Line(
                previousX, previousY,
                (short)m_width, (short)m_height)
                , Color.White, false, true);

            // update display

            m_video.Update();
        }

        private void simulate()
        {
            m_cycles += 1;
            
            for (int personId = 0; personId < m_numberOfPeople; personId++)
            {
                Person a = m_people[personId];

                if (a.Money > 0 || m_allowNegativeMoney)
                {
                    Person b = getRandomPerson(personId);

                    a.Money -= 1;
                    b.Money += 1;
                }
            }

            Video.WindowCaption = string.Format("Cycles: {0}", m_cycles);
        }

        private void updatePeak(int personId)
        {
            if (m_peakValues[personId] <= m_people[personId].Money)
            {
                // set wait time
                m_peakDelays[personId] = m_peakDelay;

                // push peak to new max
                m_peakValues[personId] = m_people[personId].Money;
            }
            else
            {
                if (m_peakDelays[personId] > 0)
                {
                    // pause before moving, decrement wait time
                    m_peakDelays[personId] -= 1;
                }
                else
                {
                    // pause has timed out, decrement value
                    m_peakValues[personId] -= 1;

                    // set new wait time
                    m_peakDelays[personId] = m_peakSpeed;
                }
            }
        }

        private Person getRandomPerson(int notId)
        {
            int selectedId = notId;
            do
            {
                selectedId = m_random.Next(0, m_numberOfPeople);
            } while (selectedId == notId);

            return m_people[selectedId];
        }

        private void Events_Quit(object sender, QuitEventArgs e)
        {
            Events.QuitApplication();
        }

        public void Run()
        {
            Events.Run();
        }
    }
}
