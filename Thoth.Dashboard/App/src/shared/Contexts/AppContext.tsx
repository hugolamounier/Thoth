import React, { createContext, useState } from 'react';
import FeatureManager from '../../pages/featureManager';

type AppContextProps = {
  currentPage: JSX.Element;
  setCurrentPage: (state: JSX.Element) => void;
};

const defaultValues = {
  currentPage: <FeatureManager listingFeatures="active" />,
};

export const AppContext = createContext({} as AppContextProps);
export const AppContextProvider = ({ children }: { children: JSX.Element }) => {
  const [currentPage, setCurrentPage] = useState<JSX.Element>(defaultValues.currentPage);
  return (
    <AppContext.Provider value={{ currentPage, setCurrentPage }}>{children}</AppContext.Provider>
  );
};
