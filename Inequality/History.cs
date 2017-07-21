namespace Inequality
{
    class History
    {
        private int m_numberOfPeople;

        private int m_itemsToKeep;

        private int[][] m_history;

        public History(int numberOfPeople, int itemsToKeep)
        {
            m_numberOfPeople = numberOfPeople;
            m_itemsToKeep = itemsToKeep;

            m_history = new int[m_numberOfPeople][];

            for (int i = 0; i < m_numberOfPeople; i++)
            {
                m_history[i] = new int[m_itemsToKeep];
            }
        }

        public void Push(int id, int value)
        {
            // shuffle

            for (int i = 1; i < m_itemsToKeep; i++)
            {
                m_history[id][i - 1] = m_history[id][i];
            }

            m_history[id][m_itemsToKeep - 1] = value;
        }

        public int Get(int id, int item)
        {
            return m_history[id][item];
        }
    }
}
