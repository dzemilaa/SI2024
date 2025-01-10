import React, { useContext, useEffect, useState } from 'react';
import './Cart.css';
import { StoreContext } from '../../context/StoreContext';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const Cart = () => {
  const { getTotalCartAmount, url } = useContext(StoreContext);
  const [cartData, setCartData] = useState([]);
  const navigate = useNavigate();
  const userId = localStorage.getItem('userId');

  // Funkcija za dohvat podataka o korpi sa backend-a
  const fetchCartItems = async () => {
    try {
      const response = await axios.get(`https://localhost:44376/api/Cart/get/${userId}`);
      console.log("Response from cart API:", response.data);
      setCartData(response.data || []);
    } catch (error) {
      console.error("Error fetching cart items:", error);
    }
  };

  useEffect(() => {
    if (userId) {
      fetchCartItems();
    }
  }, [userId]);

  // Funkcija za uklanjanje proizvoda iz korpe
  const handleRemove = async (productId) => {
    try {
      await axios.post('https://localhost:44376/api/Cart/remove', {
        ProductId: productId,
        UserId: userId,
      });
      fetchCartItems();  // Ponovo učitaj korpu nakon uklanjanja proizvoda
    } catch (error) {
      console.error("Error removing product from cart:", error);
    }
  };

  return (
    <div className='cart'>
      <div className="cart-items">
        <div className="cart-items-title">
          <p>Image</p>
          <p>Title</p>
          <p>Price</p>
          <p>Quantity</p>
          <p>Total</p>
          <p>Remove</p>
        </div>
        <br />
        <hr />
        {cartData.length > 0 ? (
          cartData.map((item) => (
            <div key={item.productId}>
              <div className='cart-items-title cart-items-item'>
                <img src={`https://localhost:44376/images/${item.image}`} alt={item.name} />
                <p>{item.name}</p>
                <p>${item.price}</p>
                <p>{item.quantity}</p>
                <p>${item.price * item.quantity}</p>
                <p onClick={() => handleRemove(item.productId)} className='cross'>x</p>
              </div>
              <hr />
            </div>
          ))
        ) : (
          <p>Your cart is empty</p>
        )}
      </div>

      <div className="cart-bottom">
        <div className="cart-total">
          <h2>Totals</h2>
          <div>
            <div className="cart-total-details">
              <p>Subtotal</p>
              <p>${getTotalCartAmount()}</p>
            </div>
            <hr />
            <div className="cart-total-details">
              <p>Delivery Fee</p>
              <p>${getTotalCartAmount() === 0 ? 0 : 2}</p>
            </div>
            <hr />
            <div className="cart-total-details">
              <b>Total</b>
              <b>${getTotalCartAmount() === 0 ? 0 : getTotalCartAmount() + 2}</b>
            </div>
          </div>
          <button onClick={() => navigate('/order')}>PROCEED TO CHECKOUT</button>
        </div>
      </div>
    </div>
  );
};

export default Cart;
