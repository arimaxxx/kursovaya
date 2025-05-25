const express = require('express');
const cors = require('cors');


const authRoutes = require('./routes/auth');
const transactionsRoutes = require('./routes/transactions');
const categoriesRoutes = require('./routes/categories'); 

const app = express();
app.use(cors());
app.use(express.json());
app.use('/api/categories', categoriesRoutes);
app.use('/api/auth', authRoutes);
app.use('/api/transactions', transactionsRoutes);

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`Server started on http://localhost:${PORT}`);
});
