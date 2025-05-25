const express = require('express');
const router = express.Router();
const db = require('../db');
const auth = require('../middleware/auth');

// Получить транзакции текущего пользователя
router.get('/', auth, async (req, res) => {
  try {
    const result = await db.query(
      'SELECT * FROM transactions WHERE user_id = $1 ORDER BY date DESC',
      [req.user.userId]
    );
    res.json(result.rows);
  } catch (e) {
    console.error(e);
    res.status(500).send('Ошибка сервера');
  }
});

// Добавить транзакцию
router.post('/', auth, async (req, res) => {
  const { amount, description, category_id, account_id } = req.body;

  try {
    const result = await db.query(
      'INSERT INTO transactions (amount, description, category_id, account_id, user_id) VALUES ($1, $2, $3, $4, $5) RETURNING *',
      [amount, description, category_id, account_id, req.user.userId]
    );
    res.status(201).json(result.rows[0]);
  } catch (e) {
    console.error(e);
    res.status(500).send('Ошибка при добавлении транзакции');
  }
});

// 🔁 Обновить транзакцию
router.put('/:id', auth, async (req, res) => {
  const { id } = req.params;
  const { amount, description, category_id } = req.body;

  try {
    const result = await db.query(
      `UPDATE transactions
       SET amount = $1, description = $2, category_id = $3
       WHERE id = $4 AND user_id = $5
       RETURNING *`,
      [amount, description, category_id, id, req.user.userId]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({ msg: 'Транзакция не найдена или доступ запрещён' });
    }

    res.json(result.rows[0]);
  } catch (e) {
    console.error(e);
    res.status(500).send('Ошибка при обновлении транзакции');
  }
});

router.delete('/:id', auth, async (req, res) => {
  const { id } = req.params;

  try {
    const result = await db.query(
      'DELETE FROM transactions WHERE id = $1 AND user_id = $2 RETURNING *',
      [id, req.user.userId]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({ msg: 'Транзакция не найдена или доступ запрещён' });
    }

    res.json({ msg: 'Удалено успешно' });
  } catch (e) {
    console.error(e);
    res.status(500).send('Ошибка при удалении транзакции');
  }
});

module.exports = router;
