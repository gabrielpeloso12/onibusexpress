import { Route, Routes } from 'react-router-dom'
import { Header } from './components/Header'
import { SearchPage } from './pages/SearchPage'
import { SeatSelectionPage } from './pages/SeatSelectionPage'
import { CheckoutPage } from './pages/CheckoutPage'
import { BookingLookupPage } from './pages/BookingLookupPage'
import { NotFoundPage } from './pages/NotFoundPage'

export function App() {
  return (
    <>
      <Header />
      <Routes>
        <Route path="/" element={<SearchPage />} />
        <Route path="/viagens/:tripId/assentos" element={<SeatSelectionPage />} />
        <Route path="/checkout" element={<CheckoutPage />} />
        <Route path="/consulta" element={<BookingLookupPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </>
  )
}
