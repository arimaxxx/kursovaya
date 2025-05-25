const express = require('express');
const router = express.Router();
const db = require('../db');
const auth = require('../middleware/auth');

router.get('/', auth, async (req, res) => {
  try {
    const result = await db.query(
      'SELECT id, name FROM categories WHERE user_id = $1',
      [req.user.userId]
    );
    res.json(result.rows);
  } catch (e) {
    console.error(e);
    res.status(500).send('Ошибка сервера при получении категорий');
  }
});

module.exports = router;
