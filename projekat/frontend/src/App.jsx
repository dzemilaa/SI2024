import React from 'react'
import Navbar from './components/Navbar/Navbar'
import { Route, Routes } from 'react-router-dom'
import Home from './pages/home/Home'
import Cart from './pages/Cart/Cart'
import PlaceOrder from './pages/PlaceOrder/PlaceOrder'
import Footer from './components/Footer/Footer'


const App = () => {
  return (
    <>
    <div className='app'>
      <Navbar> </Navbar>
      <Routes>
        <Route path='/' element={<Home></Home>} />
        <Route path='/cart' element={<Cart></Cart>} />
        <Route path='/order' element={<PlaceOrder></PlaceOrder>} />
      </Routes>
    </div>
    <Footer> </Footer>
    </>
  )
}

export default App
