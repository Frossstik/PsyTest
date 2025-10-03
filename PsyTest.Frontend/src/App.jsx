import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import { ProtectedRoute } from "./components/ProtectedRoute";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Profile from "./pages/Profile";
import EditProfile from "./pages/EditProfile";
import LuscherTest from "./pages/Tests/LuscherTest";
import Test from "./pages/Test";
import Result from "./pages/Results";
import PbqTest from "./pages/tests/PbqTest";

export default function App() {
    return (

        <AuthProvider>
            <Router>
                <Routes>
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route
                        path="/"
                        element={
                            <ProtectedRoute>
                                <Home />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/profile"
                        element={
                            <ProtectedRoute>
                                <Profile />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/profile/edit"
                        element={
                            <ProtectedRoute>
                                <EditProfile />
                            </ProtectedRoute>
                        }
                    />
                    <Route path="/tests/:id" element={<Test />} />
                    <Route path="/tests/:id/luscher" element={
                        <ProtectedRoute>
                            <LuscherTest />
                        </ProtectedRoute>
                    }
                    />
                    <Route path="/tests/:id/pbq" element={
                        <ProtectedRoute>
                            <PbqTest />
                        </ProtectedRoute>
                    }
                    />
                    <Route path="/results/:sessionId" element={<Result />} />
                </Routes>
            </Router>
        </AuthProvider>
    );
}
