import { createContext, useEffect, useState } from "react";
import axios from "axios";

export const StoreContext = createContext(null);

const StoreContextProvider = (props) => {
  const [cartItems, setCartItems] = useState({});
  const url = "https://localhost:44376";
  const [token, setToken] = useState(""); 
  const userId = localStorage.getItem("userId"); 
  const [food_list, setFoodList] = useState([]);  

  useEffect(() => {
    async function loadData() {
      await fetchFoodList();  
      if (token && userId) {
        localStorage.setItem("authToken", token);
        localStorage.setItem("userId", userId);
      }
    }
    loadData();
  }, [token, userId]);  

  // Funkcija za dodavanje proizvoda u korpu
  const addToCart = (productId) => {
    const userId = localStorage.getItem("userId");
    if (!userId) {
      alert("User ID is required to add items to your cart.");
      return;
    }

    // Provera da li je proizvod već u korpi i povećanje broja proizvoda
    setCartItems((prevItems) => {
      const updatedItems = { ...prevItems };
      updatedItems[productId] = (updatedItems[productId] || 0) + 1;
      return updatedItems;
    });

    // Poslat ćemo zahtev na API za dodavanje proizvoda u korpu
    axios
      .post('https://localhost:44376/api/Cart/add', {
        ProductId: productId,
        UserId: userId,
      })
      .then((response) => {
        if (response.status === 200) {
          console.log("Product added to cart:", response.data);
        } else {
          console.error("Error adding to cart:", response);
          alert("There was an issue adding the item to your cart. Please try again.");
        }
      })
      .catch((error) => {
        console.error("Error adding to cart:", error);
        alert("There was an error adding the item to your cart. Please check your connection or try again later.");
      });
  };

  const removeFromCart = (productId) => {
    setCartItems((prevItems) => {
      const updatedItems = { ...prevItems };
      
      // Ako je broj proizvoda veći od 1, smanji ga za 1, inače ukloni proizvod iz korpe
      if (updatedItems[productId] > 1) {
        updatedItems[productId] -= 1;
      } else if (updatedItems[productId] === 1) {
        // Ako je količina 1, ukloni proizvod iz korpe
        delete updatedItems[productId];
      }
      
      // Ovdje šaljemo API poziv za ažuriranje korpe na serveru
      axios
        .post('https://localhost:44376/api/Cart/remove', {
          ProductId: productId,
          UserId: localStorage.getItem("userId"),
          Quantity: updatedItems[productId] || 0,
        })
        .then((response) => {
          if (response.status === 200) {
            console.log("Cart updated:", response.data);
          } else {
            console.error("Error updating cart:", response);
          }
        })
        .catch((error) => {
          console.error("Error updating cart:", error);
        });
  
      return updatedItems;
    });
  };
  

  const getTotalCartAmount = () => {
    if (!food_list || food_list.length === 0 || !cartItems) {
      return 0;
    }

    let totalAmount = 0;
    for (const item in cartItems) {
      if (cartItems[item] > 0) {
        let itemInfo = food_list.find((product) => product._id == item);
        if (itemInfo && itemInfo.price) {
          totalAmount += itemInfo.price * cartItems[item];
        }
      }
    }
    return totalAmount;
  };

  const fetchFoodList = async () => {
    try {
      const response = await axios.get("https://localhost:44376/api/FetchProduct/list");
      const data = response.data.data.map((item) => ({
        ...item,
        _id: item.productId,  
      }));
      console.log("Fetched food list:", data);
      setFoodList(data);  
    } catch (error) {
      console.error("Error fetching food list:", error);
    }
  };

  const fetchCartItems = async () => {
    try {
      const userId = localStorage.getItem('userId');  
      if (!userId) {
        alert("User ID is required to fetch cart items.");
        return;
      }
  
      const response = await axios.get(`https://localhost:44376/api/Cart/get/${userId}`);
      if (response.data) {
        const cartData = response.data.reduce((acc, item) => {
          acc[item.ProductId] = item.Quantity;
          return acc;
        }, {});
  
        setCartItems(cartData);  
      }
    } catch (error) {
      console.error("Error fetching cart items:", error);
      alert("There was an error fetching the cart items. Please try again later.");
    }
  };

  const contextValue = {
    food_list,
    cartItems,
    setCartItems,
    addToCart,
    removeFromCart,
    getTotalCartAmount,
    url,
    token, 
    setToken,
  };

  return (
    <StoreContext.Provider value={contextValue}>
      {props.children}
    </StoreContext.Provider>
  );
};

export default StoreContextProvider;
