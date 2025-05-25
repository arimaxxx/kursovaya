const express = require('express');
const router = express.Router();
const db = require('../db');
const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
require('dotenv').config();

const JWT_SECRET = process.env.JWT_SECRET;

// Регистрация
router.post('/register', async (req, res) => {
  const { name, email, password } = req.body;
  try {
    const existing = await db.query('SELECT * FROM users WHERE email = $1', [email]);
    if (existing.rows.length > 0) {
      return res.status(400).json({ msg: 'Пользователь уже существует' });
    }

    const salt = await bcrypt.genSalt(10);
    const hashedPassword = await bcrypt.hash(password, salt);

    const newUser = await db.query(
      'INSERT INTO users (name, email, password) VALUES ($1, $2, $3) RETURNING id, name, email',
      [name, email, hashedPassword]
    );

    const userId = newUser.rows[0].id;

    // ✅ Копировать базовые категории новому пользователю
    await db.query(`
      INSERT INTO categories (name, type, user_id)
      SELECT name, type, $1 FROM default_categories
    `, [userId]);

    const token = jwt.sign({ userId }, JWT_SECRET, { expiresIn: '1h' });

    res.json({ token, user: newUser.rows[0] });
  } catch (e) {
    console.error(e);
    res.status(500).send('Ошибка сервера');
  }
});

// Вход
router.post('/login', async (req, res) => {
  const { email, password } = req.body;
  try {
    const userResult = await db.query('SELECT * FROM users WHERE email = $1', [email]);
    if (userResult.rows.length === 0) {
      return res.status(400).json({ msg: 'Неверный email или пароль' });
    }
    const user = userResult.rows[0];

    const isMatch = await bcrypt.compare(password, user.password);
    if (!isMatch) {
      return res.status(400).json({ msg: 'Неверный email или пароль' });
    }

    const token = jwt.sign({ userId: user.id }, JWT_SECRET, { expiresIn: '1h' });

    res.json({ token, user: { id: user.id, name: user.name, email: user.email } });
  } catch (e) {
    console.error(e);
    res.status(500).send('Ошибка сервера');
  }
});

module.exports = router;
